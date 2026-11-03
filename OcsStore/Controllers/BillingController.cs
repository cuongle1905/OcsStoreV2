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
    public class BillingController: Controller
    {
        private MyDbContext _context;

        public BillingController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult GetCustomerBillings(DataSourceLoadOptions loadOptions)
        {
            var data = _context.CustomerBillingViews.ToArray();
            var result = DataSourceLoader.Load(data, loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult GetCustomer(int id)
        {
            var data = _context.Customers.FirstOrDefault(i => i.Id == id);
            return Ok(data);
        }

        public List<Customer> GetCustomers()
        {
            var data = _context.Customers.ToList();
            return data;
        }

        [HttpPost]
        public IActionResult GetCustomerTransactionViews(short customerId, DataSourceLoadOptions loadOptions)
        {
            var data = _context.CustomerTransactionViews.Where(i => i.Customer == customerId).ToArray();
            var result = DataSourceLoader.Load(data, loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult EditPaymentTransactionDateTime(int id, DateTime date, string time)
        {
            date = TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local);
            date = new DateTime(date.Year, date.Month, date.Day);
            DateOnly dateOnly = new DateOnly(date.Year, date.Month, date.Day);

            var customerTran = _context.CustomerTransactions.FirstOrDefault(i => i.Type == CustomerTransactionType.Payment && i.Id == id);
            if (customerTran != null)
            {
                var paymentId = customerTran.MainId;
                var payment = _context.Payments.FirstOrDefault(i => i.Id == paymentId);
                if (payment != null)
                {
                    var paymentDetails = _context.PaymentDetails.Where(i => i.Payment == paymentId).ToArray();
                    foreach (PaymentDetail d in paymentDetails)
                    {
                        var bill = _context.Bills.FirstOrDefault(i => i.Id == d.Bill);
                        if (bill != null)
                        {
                            if (bill.Date > date)
                                return BadRequest("Payment date < Bill date");

                            if (bill.Date == date && bill.Time.CompareTo(time) > 0)
                                return BadRequest("Payment time < Bill time");
                        }
                    }

                    _context.Database.BeginTransaction();
                    customerTran.Date = dateOnly;
                    customerTran.Time = time;
                    customerTran.Ordinal = DB.GetNewTransactionOrdinal(_context, date, time, customerTran.Id);
                    _context.CustomerTransactions.Update(customerTran);
                    _context.SaveChanges();
                    DBBilling.UpdateCustomerTransactionDebt(_context, customerTran.Customer, 0);

                    payment.Date = dateOnly;
                    payment.Time = time;
                    _context.Payments.Update(payment);
                    _context.SaveChanges();
                    _context.Database.CommitTransaction();
                }
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult SaveCustomerPayment(int customerId, DateTime date, string time)
        {
            date = TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local);
            date = new DateTime(date.Year, date.Month, date.Day);

            var bills = _context.Bills.Where(i => !i.Deleted && !(i.Paid ?? false) && i.Customer == customerId).ToArray();
            var userId = Session.UserId(Request);
            foreach (Bill bill in bills)
            {
                DBBilling.CreatePayment(_context, bill, userId, date, time);
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult SaveBillPayment(int billId, DateTime date, string time)
        {
            date = TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local);
            date = new DateTime(date.Year, date.Month, date.Day);

            var bill = _context.Bills.FirstOrDefault(i => !i.Deleted && !(i.Paid ?? false) && i.Id == billId);
            if (bill != null)
            {
                var userId = Session.UserId(Request);
                DBBilling.CreatePayment(_context, bill, userId, date, time);
            }
            return Ok();
        }
    }
}
