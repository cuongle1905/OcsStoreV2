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
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public IActionResult GetMonthReport(int month, int year)
        {
            var result = new MonthReport();
            int prevMonth = (month == 1 ? 12 : month - 1);
            int prevYear = (month == 1 ? year - 1 : year);
            result.PrevMonth = prevMonth;

            result.SalesTotal = _context.Bills.Where(i => i.Date.Month == month && i.Date.Year == year && i.Deleted == false).Sum(j => j.TotalValue);
            result.PrevSalesTotal = _context.Bills.Where(i => i.Date.Month == prevMonth && i.Date.Year == prevYear && i.Deleted == false).Sum(j => j.TotalValue);

            result.SalesQuantity = (decimal)_context.BillDetailViews.Where(i => i.Date.Month == month && i.Date.Year == year).Sum(j => j.QuantityPerBu);
            result.PrevSalesQuantity = (decimal)_context.BillDetailViews.Where(i => i.Date.Month == prevMonth && i.Date.Year == prevYear).Sum(j => j.QuantityPerBu);

            result.CostPercentage = GetCostPercentage(month, year);
            result.PrevCostPercentage = GetCostPercentage(prevMonth, prevYear);

            //result.CustomerDebtTotal = _context.Bills.Where(i => i.Date >= fromDate && i.Date <= toDate && i.Deleted == false && i.Paid == false).Sum(j => j.TotalValue);
            result.TotalCost = GetTotalCost(month, year);
            result.PrevTotalCost = GetTotalCost(prevMonth, prevYear);

            return Ok(result);
        }

        private decimal GetCostPercentage(int month, int year)
        {
            DateTime date = new DateTime(year, month, 1).AddMonths(1);
            var itemDetails = _context.BillDetailViews.Where(i => i.Date.Month == month && i.Date.Year == year).GroupBy(e => new { e.Item }).Select(g => new { g.Key.Item, Quantity = g.Sum(e => e.QuantityPerBu), Value = g.Sum( e => e.Value ) }).ToArray();

            var totalValue = itemDetails.Sum(i => i.Value);

            if (totalValue > 0)
            {
                decimal totalMaterialCost = 0;
                foreach (var itemDetail in itemDetails)
                {
                    var materials = _context.ItemMaterials.Where(i => i.Item == itemDetail.Item).ToArray();
                    foreach (var material in materials)
                    {
                        var subMaterials = _context.ItemMaterials.Where(i => i.Item == material.Material).ToArray(); 
                        if (subMaterials.Length > 0)
                        {
                            foreach (var subMaterial in subMaterials)
                            {
                                totalMaterialCost += GetMaterialPrice(date, subMaterial.Material) * subMaterial.Quantity * material.Quantity * (decimal)itemDetail.Quantity;
                            }
                        }
                        else
                        {
                            totalMaterialCost += GetMaterialPrice(date, material.Material) * material.Quantity * (decimal)itemDetail.Quantity;
                        }
                    }
                }
                return totalMaterialCost * 100 / totalValue;
            }
            return 0;
        }

        private decimal GetTotalCost(int month, int year)
        {
            return _context.ReceivingDetailViews.Where(i => i.Date.Month == month && i.Date.Year == year).Sum(i => i.Price * i.Quantity) + _context.Expenses.Where(i => i.Date.Month == month && i.Date.Year == year).Sum(i => i.Amount);
        }

        private decimal GetMaterialPrice(DateTime date, int materialId)
        {
            var lastReceiving = _context.ReceivingDetailViews.Where(i => i.Date < date && i.Item == materialId).OrderByDescending(i => i.Date).FirstOrDefault();
            if (lastReceiving != null)
                return lastReceiving.Price;

            return 0;
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
