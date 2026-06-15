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
            var materials = _context.ItemMaterialViews.Where(i => i.Item == itemId).ToArray();
            List<ProcessingLotInputView> details = new List<ProcessingLotInputView>();
            foreach (var m in materials)
            {
                var detail = new ProcessingLotInputView() { Item = m.Material, ItemName = m.Name, Unit = m.Unit, UnitName = m.UnitName, Lot = m.Lot, Year = (sbyte)(DateTime.Today.Year % 100), UseLot = m.UseLot, ItemType = m.ItemType, Soh = m.Soh, MaterialQuantity = m.Quantity };
                details.Add(detail);
            }
            return Ok(details);
        }

        [HttpPost]
        public IActionResult Save(DateTime date, string time, int itemId, decimal quantity, ProcessingLotInputView[] details)
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

            var item = _context.ItemViews.FirstOrDefault(i => i.Id == itemId);
            var processing = new Processing() { Id = processingId, Year = (sbyte)(date.Year % 100), Item = itemId, Quantity = quantity, Date = date, Time = time, User = Session.UserId(Request) };


            if (item.UseLot)
                processing.Lot = date.ToString("ddMM");

            _context.Processings.Add(processing);

            int inputId;
            try
            {
                inputId = _context.ProcessingInputs.Max(i => i.Id);
            }
            catch
            {
                inputId = 0;
            }

            int lotInputId;
            try
            {
                lotInputId = _context.ProcessingLotInputs.Max(i => i.Id) + 1;
            }
            catch
            {
                lotInputId = 1;
            }

            foreach (var detail in details)
            {
                if (detail.Lot == null || detail.Lot == "")
                {
                    ++inputId;
                    var processingInput = new ProcessingInput() { Id = inputId, Processing = processingId, Item = detail.Item, Unit = detail.Unit, Quantity = detail.Quantity, Note = detail.Note };
                    _context.ProcessingInputs.Add(processingInput);

                    if (!detail.UseLot)
                    {
                        var processingLotInput = new ProcessingLotInput() { Id = lotInputId++, Input = inputId, Lot = null, Year = detail.Year, Quantity = detail.Quantity, Note = detail.Note };
                        _context.ProcessingLotInputs.Add(processingLotInput);
                    }
                }
                else
                {
                    var processingLotInput = new ProcessingLotInput() { Id = lotInputId++, Input = inputId, Lot = detail.Lot, Year = detail.Year, Quantity = detail.Quantity, Note = detail.Note };
                    _context.ProcessingLotInputs.Add(processingLotInput);
                }
            }

            _context.SaveChanges();

            _context.Database.ExecuteSqlRaw("call calculate_strans_processing(" + processingId + ");");

            return Ok();
        }
    }
}
