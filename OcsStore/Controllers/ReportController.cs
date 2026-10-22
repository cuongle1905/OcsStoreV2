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
    public class ReportController : Controller
    {
        private MyDbContext _context;

        public ReportController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult GetMainReport(DateTime fromDate, DateTime toDate)
        {
            var result = new MainReport();
            result.BillTotal = _context.Bills.Where(i => i.Date >= fromDate && i.Date <= toDate && i.Deleted == false).Sum(j => j.TotalValue);
            result.Stock1Value = GetStockValue(toDate, 1);
            result.Stock2Value = GetStockValue(toDate, 2);
            result.CustomerDebtTotal = _context.Bills.Where(i => i.Date >= fromDate && i.Date <= toDate && i.Deleted == false && i.Paid == false).Sum(j => j.TotalValue);
            result.RecevingTotal = _context.ReceivingDetailViews.Where(i => i.Date >= fromDate && i.Date <= toDate).Sum(i => i.Price * i.Quantity);
            result.TotalExpense = _context.Expenses.Where(i => i.Date >= fromDate && i.Date <= toDate).Sum(i => i.Amount);
            return Ok(result);
        }

        private decimal GetStockValue(DateTime date, sbyte itemGroupId)
        {
            var stocks = DBStock.GetStocks(_context, date, itemGroupId, "");
            return stocks.Sum(i => i.Value) ?? 0;
        }

        public decimal GetBillTotal()
        {
            return _context.Bills.Where(i => i.Deleted == false).Sum(j => j.TotalValue);
        }

        public decimal GetCustomerDebtTotal()
        {
            return _context.Bills.Where(i => i.Deleted == false && i.Paid == false).Sum(j => j.TotalValue);
        }

        public decimal GetStockValue(short itemGroupId)
        {
            return _context.StockViews.Where(i => i.ItemGroup == itemGroupId && i.Soh > 0).Sum(i => i.Value) ?? 0;
        }

        public decimal GetReceivingTotalValue()
        {
            return _context.ReceivingDetails.Sum(i => i.Price * i.Quantity);
        }

        public decimal GetTotalExpense()
        {
            return _context.Expenses.Sum(i => i.Amount);
        }
    }
}
