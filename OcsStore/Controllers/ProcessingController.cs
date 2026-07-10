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
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public IActionResult GetProcessingInputViews(sbyte itemGroup, DataSourceLoadOptions loadOptions)
        {
            var data = _context.ProcessingInputViews.Where(i => i.ItemGroup == itemGroup).ToArray();
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
        public IActionResult GetProcessingViews(sbyte itemGroup, DataSourceLoadOptions loadOptions)
        {
            var data = _context.ProcessingViews.Where(i => i.ItemGroup == itemGroup).ToArray();
            var isAdmin = Session.IsAdmin(Request);
            var userId = Session.UserId(Request);
            foreach (var record in data)
            {
                record.AllowDelete = isAdmin || (record.User == userId && record.DateCreated == DateTime.Today);
            }
            var result = DataSourceLoader.Load(data, loadOptions);
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
            List<ProcessingInputView> details = new List<ProcessingInputView>();
            foreach (var m in materials)
            {
                var detail = new ProcessingInputView() { Item = m.Material, ItemName = m.Name, Unit = m.Unit, UnitName = m.UnitName, MaterialQuantity = m.Quantity, Soh = m.Soh };
                details.Add(detail);
            }
            return Ok(details);
        }

        private int GetNewProcessingId()
        {
            int processingId;
            try
            {
                processingId = _context.Processings.Max(i => i.Id) + 1;
            }
            catch
            {
                processingId = 1;
            }
            return processingId;
        }

        private int GetNewProcessingInputId()
        {
            int inputId;
            try
            {
                inputId = _context.ProcessingInputs.Max(i => i.Id) + 1;
            }
            catch
            {
                inputId = 1;
            }
            return inputId;
        }

        [HttpPost]
        public IActionResult SaveRawProcessing(Processing processing, decimal materialQuantity)
        {
            var tranId = DB.GetNewId(_context, "store_transaction");
            SaveNewProcessing(processing, tranId++);

            var materialId = _context.ItemMaterials.FirstOrDefault(i => i.Item == processing.Item).Material;

            int inputId = GetNewProcessingInputId();

            var processingInput = new ProcessingInput() { Id = inputId, Processing = processing.Id, Item = materialId, Quantity = materialQuantity };
            _context.ProcessingInputs.Add(processingInput);

            _context.SaveChanges();
            DBProcessing.UpdateStoreTransactionForProcessingInput(_context, tranId++, processing, processingInput);

            DBProcessing.UpdateStoreTransactionForProcessingOutput(_context, tranId, processing); // Call at the end to calculate correct price

            return Ok();
        }

        private void SaveNewProcessing(Processing processing, int tranId)
        {
            processing.Id = GetNewProcessingId();
            processing.Date = Common.GetLocalDateWithoutTime(processing.Date); // Remove hour, minute...
            processing.Store = 1;
            processing.Unit = 1;
            processing.User = Session.UserId(Request);
            processing.DateCreated = DateTime.Today;
            processing.TimeCreated = DateTime.Now.ToString("HH:mm");

            _context.Processings.Add(processing);
            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult Save(Processing processing, ProcessingInputView[] details)
        {
            _context.Database.BeginTransaction();

            var tranId = DB.GetNewId(_context, "store_transaction");
            SaveNewProcessing(processing, tranId++);

            int inputId = GetNewProcessingInputId() - 1;

            var lotInputId = DB.GetNewId(_context, "processing_input");

            ProcessingInput processingInput = new ProcessingInput();
            try
            {
                foreach (var detail in details)
                {
                    ++inputId;
                    processingInput = new ProcessingInput() { Id = inputId, Processing = processing.Id, Item = detail.Item, Unit = detail.Unit, Quantity = detail.Quantity };
                    _context.ProcessingInputs.Add(processingInput);

                    _context.SaveChanges();
                    DBProcessing.UpdateStoreTransactionForProcessingInput(_context, tranId++, processing, processingInput);
                }

                DBProcessing.UpdateStoreTransactionForProcessingOutput(_context, tranId, processing); // Call at the end to calculate correct price
            }
            catch (Exception ex)
            {
                _context.Database.RollbackTransaction();
                return BadRequest(ex.Message);
            }

            _context.Database.CommitTransaction();
            return Ok(processing.Id);
        }

        [HttpPost]
        public IActionResult DeleteProcessing(int id)
        {
            var processing = _context.Processings.FirstOrDefault(i => i.Id == id);
            if (processing != null)
            {
                _context.Database.BeginTransaction();
                try
                {
                    DBProcessing.DeleteStoreTransactionsForProcessing(_context, processing);
                    _context.Processings.Remove(processing);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    _context.Database.RollbackTransaction();
                    return BadRequest(ex.Message);
                }
                _context.Database.CommitTransaction();
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult EditDateTime(int id, DateTime date, string time)
        {
            string errorMesssage;
            if (DBProcessing.EditProcessingDateTime(_context, id, date, time, out errorMesssage))
                return Ok();

            return BadRequest(errorMesssage);
        }

        public ProcessingView GetProcessingView(int id)
        {
            return _context.ProcessingViews.AsNoTracking().FirstOrDefault(i => i.Id == id);
        }
    }
}
