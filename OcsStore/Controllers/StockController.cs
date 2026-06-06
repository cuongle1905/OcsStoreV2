using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OcsStore.Models;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace OcsStore.Controllers
{
    [Route("api/[controller]/[action]")]
    public class StockController: Controller
    {
        private MyDbContext _context;

        public StockController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult GetStocks(DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.StockViews, loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult GetStockCard(int itemId, string lot, short year, DataSourceLoadOptions loadOptions)
        {
            IQueryable<StockCardView> data;
            if (string.IsNullOrEmpty(lot) || year <= 0)
                data = _context.StockCardViews.Where(i => i.Item == itemId);
            else
                data = _context.StockCardViews.Where(i => i.Item == itemId && i.Lot == lot && i.Year == year);

            var result = DataSourceLoader.Load(data, loadOptions);
            return Ok(result);
        }
    }
}
