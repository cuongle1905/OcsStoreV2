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

    }
}
