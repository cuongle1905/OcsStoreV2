using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OcsStore.Models;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace OcsStore.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ProcessingController: Controller
    {
        private MyDbContext _context;

        public ProcessingController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult GetProcessings(int type, DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.ProcessingViews, loadOptions);
            return Ok(result);
        }

        public string GetProcessingName(short typeId)
        {
            return _context.ItemGroups.FirstOrDefault(i => i.Id == typeId + 1).ProcessingName;
        }

        [HttpPost]
        public IActionResult GetNewDetails(int itemId)
        {
            short storeId = 1;
            var materials = _context.ItemMaterials.Where(i => i.Item == itemId).ToArray();
            List<ProcessingInputView> details = new List<ProcessingInputView>();
            foreach (var m in materials)
            {
                //if (m.Soh > 0 && m.UseLot)
                //{
                //    var stocks = _context.StockViews.Where(i => i.Store == m.Store && i.Item == m.Item && i.Unit == m.Unit && !string.IsNullOrEmpty(i.Lot) && i.Soh > 0).ToArray();

                //    foreach (var stock in stocks)
                //    {
                //        var detailByLot = new ProcessingDetailView() { Item = m.Item, ItemName = m.ItemName, Unit = m.Unit, UnitName = m.UnitName, Lot = stock.Lot, Year = stock.Year, InOut = m.InOut, IsOutput = m.IsOutput, UseLot = m.UseLot, ItemIsInput = m.ItemIsInput, ItemIsOutput = m.ItemIsOutput, Store = m.Store, Soh = stock.Soh };
                //        details.Add(detailByLot);
                //    }
                //}
                //else
                //{
                //    var detail = new ProcessingDetailView() { Item = m.Item, ItemName = m.ItemName, Unit = m.Unit, UnitName = m.UnitName, Lot = "", Year = (sbyte)(DateTime.Today.Year % 100), InOut = m.InOut, IsOutput = m.IsOutput, UseLot = m.UseLot, ItemIsInput = m.ItemIsInput, ItemIsOutput = m.ItemIsOutput, Store = m.Store, Soh = m.Soh };
                //    details.Add(detail);
                //}
            }
            return Ok(details);
        }

        [HttpPost]
        public IActionResult Save(DateTime date, string time, int model, ProcessingDetail[] details)
        {
            date = Common.GetLocalDateWithoutTime(date); // Remove hour, minute...
            int processingId;
            try
            {
                processingId = _context.Processings.Max(i => i.Id) + 1;
            }
            catch
            {
                processingId = 1;
            }

            var processing = new Processing() { Id = processingId, Date = date, Time = time, User = Session.UserId(Request) };
            _context.Processings.Add(processing);

            int detailId;
            try
            {
                detailId = _context.ProcessingInputs.Max(i => i.Id) + 1;
            }
            catch
            {
                detailId = 1;
            }

            foreach (var detail in details)
            {
                var processingInput = new ProcessingInput() { Id = detailId++, Processing = processingId, Lot = detail.Lot, Item = detail.Item, Unit = detail.Unit, Quantity = detail.Quantity, Note = detail.Note };
                _context.ProcessingInputs.AddRange(processingInput);
            }

            _context.SaveChanges();

            _context.Database.ExecuteSqlRaw("call calculate_strans_processing(" + processingId + ");");

            return Ok();
        }
    }
}
