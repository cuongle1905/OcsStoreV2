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
        public IActionResult GetBills(int customerId, DateTime fromDate, DateTime toDate, DataSourceLoadOptions loadOptions)
        {
            fromDate = Common.GetLocalDateWithoutTime(fromDate); // Remove hour, minute...
            toDate = Common.GetLocalDateWithoutTime(toDate); // Remove hour, minute...
            var data = _context.BillViews.Where(i => i.Date >= fromDate && i.Date <= toDate && (customerId <= 0 || i.Customer == customerId));
            var result = DataSourceLoader.Load(data, loadOptions);
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

            bill.TotalValue = decimal.Round(details.Sum(i => i.Quantity * (i.Price - i.Discount)) * (100 + bill.VatPercent) / 100);
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
            var tranId = DB.GetNewId(_context, "store_transaction");

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
                    DBBilling.UpdateStoreTransactionForBillDetail(_context, tranId++, bill, detail);
                }
                catch (Exception ex)
                {
                    _context.Database.RollbackTransaction();
                    return BadRequest(ex.Message);
                }
            }

            _context.Database.CommitTransaction();

            if (isNewBill)
            {
                DBBilling.CreateCustomerTransactionsForBill(_context, bill);

                if (bill.Paid ?? false)
                {
                    DBBilling.CreatePayment(_context, bill, currentUser);
                }
            }

            return Ok();
        }

        [HttpPost]
        public IActionResult UpdateVatPercent(int billId, decimal vatPercent)
        {
            var sum = _context.BillDetails.Where(i => i.Id == billId).Sum(i => i.Quantity * (i.Price - i.Discount));
            var bill = _context.Bills.FirstOrDefault(i => i.Id == billId);
            bill.VatPercent = vatPercent;
            bill.TotalValue = decimal.Round(sum * (100 + vatPercent) / 100);
            _context.Bills.Update(bill);
            _context.SaveChanges();
            return Ok();
        }

        public Customer[] GetCustomers()
        {
            return _context.Customers.AsNoTracking().ToArray();
        }

        public List<Customer> GetCustomerList()
        {
            return _context.Customers.AsNoTracking().ToList();
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

                    DBBilling.DeletePaymentForBill(_context, bill.Id);

                    DBBilling.DeleteCustomerTransactionForBill(_context, bill.Id);

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
                DBBilling.DeletePaymentForBill(_context, bill.Id);
                if (paid)
                {
                    DBBilling.CreatePayment(_context, bill, Session.UserId(Request));
                }
                else
                {
                    bill.Paid = false;
                    bill.DatePaid = null;
                    bill.TimePaid = null;
                    bill.UserPaid = null;
                    _context.Bills.Update(bill);
                    _context.SaveChanges();
                }
            }
            return Ok();
        }

        public void UpdateMissingCustomerTransactions()
        {
            DBBilling.RemoveInvalidCustomerTransactions(_context);
            DBBilling.UpdateMissingPayments(_context);
            DBBilling.UpdateMissingCustomerTransactions(_context);
            DBBilling.UpdateCustomerTransactionsStatus(_context);
        }
    }
}
