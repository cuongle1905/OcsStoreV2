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
            var result = DataSourceLoader.Load(_context.ProcessingDetailViews.Where(i => i.Type == type), loadOptions);
            return Ok(result);
        }

        public ProcessingType GetProcessingType(int typeId)
        {
            return _context.ProcessingTypes.FirstOrDefault(i => i.Id == typeId);
        }

        public ProcessingModel[] GetModels(int typeId)
        {
            return _context.ProcessingModels.Where(i => i.Type == typeId).ToArray();
        }

        [HttpPost]
        public IActionResult GetNewDetails(int model)
        {
            var modelDetails = _context.ProcessingModelDetailViews.Where(i => i.Model == model).ToArray();
            List<ProcessingDetailView> details = new List<ProcessingDetailView>();
            foreach (var md in modelDetails)
            {
                if (!md.IsOutput && md.UseLot)
                {
                    var stocks = _context.StockViews.Where(i => i.Store == md.Store && i.Item == md.Item && i.Unit == md.Unit && !string.IsNullOrEmpty(i.Lot) && i.Soh > 0).ToArray();
                    foreach (var stock in stocks)
                    {
                        var detail = new ProcessingDetailView() { Item = md.Item, ItemName = md.ItemName, Unit = md.Unit, UnitName = md.UnitName, Lot = stock.Lot, Year = stock.Year, InOut = md.InOut, IsOutput = md.IsOutput, UseLot = md.UseLot, ItemIsInput = md.ItemIsInput, ItemIsOutput = md.ItemIsOutput, Store = md.Store, Soh = stock.Soh };
                        details.Add(detail);
                    }
                }
                else
                {
                    var detail = new ProcessingDetailView() { Item = md.Item, ItemName = md.ItemName, Unit = md.Unit, UnitName = md.UnitName, Lot = "", Year = (sbyte)(DateTime.Today.Year % 100), InOut = md.InOut, IsOutput = md.IsOutput, UseLot = md.UseLot, ItemIsInput = md.ItemIsInput, ItemIsOutput = md.ItemIsOutput, Store = md.Store, Soh = md.Soh };
                    details.Add(detail);
                }
            }
            return Ok(details);
        }

        [HttpPost]
        public IActionResult GetNewDetailsByLot(int model)
        {
            var modelDetails = _context.ProcessingModelDetailViews.Where(i => i.Model == model && i.IsOutput == false).ToArray();
            List<ProcessingDetailView> details = new List<ProcessingDetailView>();
            foreach (var md in modelDetails)
            {
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

            var processing = new Processing() { Id = processingId, Date = date, Time = time, Model = (short)model, User = Session.UserId(Request) };
            _context.Processings.Add(processing);

            int detailId;
            try
            {
                detailId = _context.ProcessingDetails.Max(i => i.Id) + 1;
            }
            catch
            {
                detailId = 1;
            }

            foreach (var detail in details)
            {
                var processingDetail = new ProcessingDetail() { Id = detailId++, Processing = processingId, Lot = detail.Lot, Item = detail.Item, Unit = detail.Unit, IsOutput = detail.IsOutput, Quantity = detail.Quantity, Note = detail.Note };
                _context.ProcessingDetails.AddRange(processingDetail);
            }

            _context.SaveChanges();

            _context.Database.ExecuteSqlRaw("call calculate_strans_processing(" + processingId + ");");

            return Ok();
        }
    }
}
