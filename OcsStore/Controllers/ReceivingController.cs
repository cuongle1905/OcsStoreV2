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
            var data = _context.ReceivingDetailViews.ToArray();
            var isAdmin = Session.IsAdmin(Request);
            var userId = Session.UserId(Request);
            foreach (var record in data)
            {
                record.AllowDelete = isAdmin || (record.User == userId && record.DateCreated == DateTime.Today);
            }
            var result = DataSourceLoader.Load(data, loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult GetNewDetails(int itemId, DataSourceLoadOptions loadOptions)
        {
            var data = _context.StockViews.Where(i => i.ItemType == Item.Receving).ToList();
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
                var detail = new ReceivingDetailView() { Item = d.Item, ItemName = d.ItemName, Unit = d.Unit, UnitName = d.UnitName };
                details.Add(detail);
            }

            return Ok(details);
        }

        [HttpPost]
        public IActionResult Save(DateTime date, string time, ReceivingDetail[] details)
        {
            date = Common.GetLocalDateWithoutTime(date); // Remove hour, minute...
            DateTime dateCreated = DateTime.Today;
            string timeCreated = DateTime.Now.ToString("HH:mm");

            var dbTran = _context.Database.BeginTransaction();

            int receivingId = DB.GetNewId(_context, "receiving");
            var receiving = new Receiving() { Id = receivingId, Store = 1, Date = date, Time = time, User = Session.UserId(Request), DateCreated = dateCreated, TimeCreated = timeCreated };
            _context.Receivings.Add(receiving);

            int detailId = DB.GetNewId(_context, "receiving_detail");
            List<ReceivingDetail> newDetails = new List<ReceivingDetail>();
            foreach (var detail in details)
            {
                var receivingDetail = new ReceivingDetail() { Id = detailId++, Receiving = receivingId, Item = detail.Item, Unit = detail.Unit, Quantity = detail.Quantity, Price = detail.Price, Note = detail.Note, Ordinal = detail.Ordinal };
                newDetails.Add(receivingDetail);
            }
            _context.ReceivingDetails.AddRange(newDetails);

            _context.SaveChanges();

            //_context.Database.ExecuteSqlRaw("call calculate_strans_receiving(" + receivingId + ");");
            DB.UpdateStockForReceiving(_context, receiving, newDetails);

            dbTran.Commit();

            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteDetail(int id)
        {
            _context.Database.ExecuteSqlRaw("call delete_receiving_detail(" + id + ");");
            return Ok();
        }
    }
}
