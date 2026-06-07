using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OcsStore.Models;
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

        [HttpPost]
        public IActionResult GetNewBillDetails()
        {
            var stocks = _context.StockViews.Where(i => i.Soh > 0).ToArray();

            List<BillDetailView> details = new List<BillDetailView>();
            foreach (var stock in stocks)
            {
                var detail = new BillDetailView() { Item = stock.Item, ItemName = stock.ItemName, Unit = stock.Unit, UnitName = stock.UnitName, Soh = stock.Soh, Ave = stock.Ave, StockUnit = stock.Unit, StockUnitName = stock.UnitName, Ordinal = 1 };

                var lastBillPrice = _context.BillDetailViews.Where(i => i.Item == stock.Item && i.Unit == stock.Unit).OrderByDescending(j => j.Date).OrderByDescending(k => k.Time).OrderByDescending(l => l.Id).FirstOrDefault();
                if (lastBillPrice != null)
                    detail.Price = lastBillPrice.Price;

                details.Add(detail);
            }
            return Ok(details);
        }


        [HttpPost]
        public IActionResult Save(DateTime date, string time, BillDetail[] details, Customer customer, bool debit)
        {
            date = Common.GetLocalDateWithoutTime(date); // Remove hour, minute...
            DateTime currentDate = DateTime.Today;
            string currentTime = DateTime.Now.ToString("HH:mm");
            short currentUser = Session.UserId(Request);

            if (customer.Id <= 0 && !string.IsNullOrEmpty(customer.Name))
            {
                try
                {
                    customer.Id = (short)(_context.Customers.Max(i => i.Id) + 1);
                }
                catch
                {
                    customer.Id = 1;
                }
                _context.Customers.Add(customer);
            }
            else
            {
                var existingCustomer = _context.Customers.FirstOrDefault(i => i.Id == customer.Id);
                if (existingCustomer != null)
                {
                    existingCustomer.Name = customer.Name;
                    existingCustomer.Phone = customer.Phone;
                    existingCustomer.Address = customer.Address;
                    existingCustomer.Email = customer.Email;
                }
                else
                {
                    customer.Id = 0; // Unknown customer
                }
                _context.Customers.Update(existingCustomer);
            }

            int billId;
            try
            {
                billId = _context.Bills.Max(i => i.Id) + 1;
            }
            catch
            {
                billId = 1;
            }

            var billTotal = details.Sum(i => i.Quantity * (i.Price - i.Discount));

            var bill = new Bill() { Id = billId, Date = date, Time = time, DateCreated = currentDate, TimeCreated = currentTime, UserCreated = currentUser, CustomerName = customer.Name, CustomerPhone = customer.Phone, CustomerAddress = customer.Address, CustomerEmail = customer.Email, Paid = !debit, TotalValue = billTotal };
            if (customer.Id > 0)
            {
                bill.Customer = customer.Id;
            }

            if (!debit)
            {
                bill.DatePaid = currentDate;
                bill.TimePaid = currentTime;
                bill.UserPaid = currentUser;
            }
            _context.Bills.Add(bill);
            
            int detailId;
            try
            {
                detailId = _context.BillDetails.Max(i => i.Id) + 1;
            }
            catch
            {
                detailId = 1;
            }

            for (int i = 0; i < details.Length; i++)
            {
                var detail = details[i];
                var billDetail = new BillDetail() { Id = detailId++, Bill = billId, Item = detail.Item, Unit = detail.Unit, Quantity = detail.Quantity, Price = detail.Price, Discount = detail.Discount, Note = detail.Note, Ordinal = i + 1 };
                _context.BillDetails.AddRange(billDetail);
            }

            _context.SaveChanges();

            _context.Database.ExecuteSqlRaw("call calculate_strans_bill(" + billId + ");");

            return Ok();
        }


        [HttpPost]
        public IActionResult GetCustomers(DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.Customers, loadOptions);
            return Ok(result);
        }
    }
}
