using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OcsStore.Models;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OcsStore.Controllers
{
    [Route("api/[controller]/[action]")]
    public class SalesController: Controller
    {
        private MyDbContext _context;

        public SalesController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult GetBills(DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.BillViews, loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult GetCustomerViews(DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.CustomerViews, loadOptions);
            return Ok(result);
        }

        public BillDetailView[] GetBillDetails(int billId)
        {
            return _context.BillDetailViews.Where(i => i.Bill == billId).AsNoTracking().ToArray();
        }

        public Bill GetBill(int billId)
        {
            var data = _context.Bills.AsNoTracking().FirstOrDefault(i => i.Id == billId);
            return data;
        }

        [HttpPost]
        public IActionResult SaveBill(Bill bill, BillDetail[] details)
        {
            bill.Date = Common.GetLocalDateWithoutTime(bill.Date); // Remove hour, minute...
            DateTime currentDate = DateTime.Today;
            string currentTime = DateTime.Now.ToString("HH:mm");
            short currentUser = Session.UserId(Request);

            _context.Database.BeginTransaction();

            bool isNewBill = (bill.Id == 0);
            if (isNewBill)
            {
                bill.Id = DB.GetNewId(_context, "bill");
            }

            bill.TotalValue = details.Sum(i => i.Quantity * (i.Price - i.Discount));
            bill.DateCreated = currentDate;
            bill.TimeCreated = currentTime;
            bill.UserCreated = currentUser;

            if (isNewBill && (bill.Paid ?? false))
            {
                bill.DatePaid = currentDate;
                bill.TimePaid = currentTime;
                bill.UserPaid = currentUser;
            }

            _context.Bills.Add(bill);

            int detailId = DB.GetNewId(_context, "bill_detail");

            for (int i = 0; i < details.Length; i++)
            {
                var detail = details[i];
                detail.Id = detailId++;
                detail.Bill = bill.Id;
                detail.Ordinal = i + 1;

                _context.BillDetails.Add(detail);
                _context.SaveChanges();

                try
                {
                    CreateBillLotDetails(bill, detail);
                }
                catch (Exception ex)
                {
                    _context.Database.RollbackTransaction();
                    return BadRequest(ex.Message);
                }
            }

            _context.Database.CommitTransaction();

            return Ok();
        }

        private void CreateBillLotDetails(Bill bill, BillDetail billDetail)
        {
            StockView[] stocks;
            var item = _context.Items.FirstOrDefault(i => i.Id == billDetail.Item);
            if (item.UseLot)
                stocks = _context.StockViews.Where(i => i.Soh > 0 && i.Item == billDetail.Item && !string.IsNullOrEmpty(i.Lot)).OrderBy(i => i.LotOrdinal).AsNoTracking().ToArray();
            else
                stocks = _context.StockViews.Where(i => i.Soh > 0 && i.Item == billDetail.Item && string.IsNullOrEmpty(i.Lot)).AsNoTracking().ToArray();

            var remainQuantity = billDetail.Quantity;
            if (billDetail.Unit != 1)
            {
                var buExchange = _context.Units.FirstOrDefault(i => i.Id == billDetail.Unit).BuExchange;
                remainQuantity *= (decimal)buExchange;
            }

            var billLotDetailId = DB.GetNewId(_context, "bill_lot_detail");
            var tranId = DB.GetNewId(_context, "store_transaction");

            for (int i = 0; i < stocks.Length; i++)
            {
                var stock = stocks[i];
                
                BillLotDetail detail = new BillLotDetail() { Id = billLotDetailId++, BillDetail = billDetail.Id };

                if (stock.Lot != null)
                   detail.Lot = stock.Lot;

                detail.Year = (sbyte)stock.Year;

                detail.Quantity = Math.Min((decimal)stock.Soh, remainQuantity);

                _context.BillLotDetails.Add(detail);
                _context.SaveChanges();

                try
                {
                    DBBilling.UpdateStoreTransactionForBillLotDetail(_context, tranId++, bill, billDetail, detail);
                }
                catch
                {
                    throw;
                }

                remainQuantity -= detail.Quantity;
                if (remainQuantity == 0)
                    break;
            }
            if (remainQuantity > 0)
            {
                throw new InvalidOperationException($"Không đủ số lượng tồn kho '{item.Name}'.");
            }

        }

        public Customer[] GetCustomers()
        {
            return _context.Customers.AsNoTracking().ToArray();
        }

        [HttpPost]
        public IActionResult GetCustomerDataSource(DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.Customers, loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult GetCustomerManagementViews(DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.CustomerManagementViews, loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult DeleteCustomer(int id)
        {
            if (_context.Bills.FirstOrDefault(i => i.Customer == id) == null)
            {
                var unit = _context.Customers.FirstOrDefault(i => i.Id == id);
                if (unit != null)
                {
                    _context.Customers.Remove(unit);
                    _context.SaveChanges();
                }
            }
            return Ok();
        }

        private void SaveCustomer(Customer unit)
        {
            if (unit.Id == 0)
            {
                try
                {
                    unit.Id = (short)(_context.Customers.Max(i => i.Id) + 1);
                }
                catch
                {
                    unit.Id = 1;
                }

                _context.Customers.Add(unit);
            }
            else
            {
                _context.Customers.Update(unit);
            }
            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult SaveCustomers(Customer[] data)
        {
            foreach (Customer unit in data)
            {
                SaveCustomer(unit);
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteBill(int id)
        {
            var bill = _context.Bills.FirstOrDefault(i => i.Id == id);
            if (bill != null)
            {
                _context.Database.BeginTransaction();
                try
                {
                    DBBilling.DeleteStoreTransactionsForBill(_context, bill);
                    _context.Bills.Remove(bill);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    _context.Database.RollbackTransaction();
                    return BadRequest(ex.Message);
                }
                _context.Database.CommitTransaction();
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult UpdatePaid(int id, bool paid)
        {
            var bill = _context.Bills.FirstOrDefault(i => i.Id == id);
            if (bill != null)
            {
                bill.Paid = paid;
                if (paid)
                {
                    bill.DatePaid = DateTime.Today;
                    bill.TimePaid = DateTime.Now.ToString("HH:mm");
                    bill.UserPaid = Session.UserId(Request);
                }
                else
                {
                    bill.DatePaid = null;
                    bill.TimePaid = null;
                    bill.UserPaid = null;
                }
                _context.Bills.Update(bill);
                _context.SaveChanges();
            }
            return Ok();
        }
    }
}
