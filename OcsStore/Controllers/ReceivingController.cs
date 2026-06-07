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
    public class ReceivingController: Controller
    {
        private MyDbContext _context;

        public ReceivingController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult GetReceivings(DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.ReceivingDetailViews, loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult GetNewDetails(int itemId, DataSourceLoadOptions loadOptions)
        {
            var data = _context.StockViews.Where(i => i.IsInput).ToList();
            if (itemId > 0)
            {
                var item = data.FirstOrDefault(i => i.Item == itemId);
                if (item != null)
                {
                    data.Remove(item);
                    data.Insert(0, item);
                }
            }

            List<ReceivingDetailView> details = new List<ReceivingDetailView>();
            foreach (var d in data)
            {
                var detail = new ReceivingDetailView() { Item = d.Item, ItemName = d.ItemName, Unit = d.Unit, UnitName = d.UnitName, Soh = d.Soh, SohWarning = d.SohWarning };
                details.Add(detail);
            }

            return Ok(details);
        }

        [HttpPost]
        public IActionResult Save(DateTime date, string time, ReceivingDetail[] details)
        {
            date = Common.GetLocalDateWithoutTime(date); // Remove hour, minute...
            int receivingId;
            try
            {
                receivingId = _context.Receivings.Max(i => i.Id) + 1;
            }
            catch
            {
                receivingId = 1;
            }

            var receiving = new Receiving() { Id = receivingId, Date = date, Time = time, User = Session.UserId(Request) };
            _context.Receivings.Add(receiving);
            
            int detailId;
            try
            {
                detailId = _context.ReceivingDetails.Max(i => i.Id) + 1;
            }
            catch
            {
                detailId = 1;
            }

            foreach (var detail in details)
            {
                var receivingDetail = new ReceivingDetail() { Id = detailId++, Receiving = receivingId, Item = detail.Item, Unit = detail.Unit, Quantity = detail.Quantity, Price = detail.Price, Note = detail.Note, Ordinal = detail.Ordinal };
                _context.ReceivingDetails.AddRange(receivingDetail);
            }

            _context.SaveChanges();

            _context.Database.ExecuteSqlRaw("call calculate_strans_receiving(" + receivingId + ");");

            return Ok();
        }
    }
}
