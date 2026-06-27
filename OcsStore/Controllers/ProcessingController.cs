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
            List<ProcessingLotInputView> details = new List<ProcessingLotInputView>();
            foreach (var m in materials)
            {
                var detail = new ProcessingLotInputView() { Item = m.Material, ItemName = m.Name, Unit = m.Unit, UnitName = m.UnitName, Lot = m.Lot, Year = (sbyte)(DateTime.Today.Year % 100), UseLot = m.UseLot, ItemType = m.ItemType, Soh = m.Soh, MaterialQuantity = m.Quantity };
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

        private int GetNewProcessingLotInputId()
        {
            int lotInputId;
            try
            {
                lotInputId = _context.ProcessingLotInputs.Max(i => i.Id) + 1;
            }
            catch
            {
                lotInputId = 1;
            }
            return lotInputId;
        }

        [HttpPost]
        public IActionResult SaveRawProcessing(Processing processing, decimal materialQuantity)
        {
            var processingId = SaveNewProcessing(processing);

            var materialId = _context.ItemMaterials.FirstOrDefault(i => i.Item == processing.Item).Material;

            int inputId = GetNewProcessingInputId();

            var processingInput = new ProcessingInput() { Id = inputId, Processing = processingId, Item = materialId, Quantity = materialQuantity };
            _context.ProcessingInputs.Add(processingInput);

            int lotInputId = GetNewProcessingLotInputId();
            var processingLotInput = new ProcessingLotInput() { Id = lotInputId++, Input = inputId, Lot = null, Year = processing.Year, Quantity = materialQuantity };
            _context.ProcessingLotInputs.Add(processingLotInput);

            _context.SaveChanges();

            _context.Database.ExecuteSqlRaw("call calculate_strans_processing(" + processingId + ");");

            return Ok();
        }

        private int SaveNewProcessing(Processing processing)
        {
            processing.Date = Common.GetLocalDateWithoutTime(processing.Date); // Remove hour, minute...
            DateTime dateCreated = DateTime.Today;
            string timeCreated = DateTime.Now.ToString("HH:mm");

            int processingId = GetNewProcessingId();

            var item = _context.ItemViews.FirstOrDefault(i => i.Id == processing.Item);
            var yy = (sbyte)(processing.Date.Year % 100);
            var newProcessing = new Processing() { Id = processingId, Year = yy, Item = processing.Item, Quantity = processing.Quantity, Date = processing.Date, Time = processing.Time, User = Session.UserId(Request), DateCreated = dateCreated, TimeCreated = timeCreated };

            _context.Processings.Add(newProcessing);
            return processingId;
        }

        [HttpPost]
        public IActionResult Save(Processing processing, ProcessingLotInputView[] details)
        {
            var processingId = SaveNewProcessing(processing);

            int inputId = GetNewProcessingInputId();

            int lotInputId = GetNewProcessingLotInputId();

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

        private void CreateBill(DateTime date, string time, BillDetail[] details, Customer customer, bool debit)
        {
            date = Common.GetLocalDateWithoutTime(date); // Remove hour, minute...
            DateTime currentDate = DateTime.Today;
            string currentTime = DateTime.Now.ToString("HH:mm");
            short currentUser = Session.UserId(Request);

            if (customer.Id <= 0 && !string.IsNullOrEmpty(customer.Name))
            {
                try
                {
                    customer.Id = (short)(_context.Customers.Max(i => i.Id) + 1);
                }
                catch
                {
                    customer.Id = 1;
                }
                _context.Customers.Add(customer);
            }
            else
            {
                var existingCustomer = _context.Customers.FirstOrDefault(i => i.Id == customer.Id);
                if (existingCustomer != null)
                {
                    existingCustomer.Name = customer.Name;
                    existingCustomer.Phone = customer.Phone;
                    existingCustomer.Address = customer.Address;
                    existingCustomer.Email = customer.Email;
                }
                else
                {
                    customer.Id = 0; // Unknown customer
                }
                _context.Customers.Update(existingCustomer);
            }

            int billId;
            try
            {
                billId = _context.Bills.Max(i => i.Id) + 1;
            }
            catch
            {
                billId = 1;
            }

            var billTotal = details.Sum(i => i.Quantity * (i.Price - i.Discount));

            var bill = new Bill() { Id = billId, Date = date, Time = time, DateCreated = currentDate, TimeCreated = currentTime, UserCreated = currentUser, CustomerName = customer.Name, CustomerPhone = customer.Phone, CustomerAddress = customer.Address, CustomerEmail = customer.Email, Paid = !debit, TotalValue = billTotal };
            if (customer.Id > 0)
            {
                bill.Customer = customer.Id;
            }

            if (!debit)
            {
                bill.DatePaid = currentDate;
                bill.TimePaid = currentTime;
                bill.UserPaid = currentUser;
            }
            _context.Bills.Add(bill);

            int detailId;
            try
            {
                detailId = _context.BillDetails.Max(i => i.Id) + 1;
            }
            catch
            {
                detailId = 1;
            }

            for (int i = 0; i < details.Length; i++)
            {
                var detail = details[i];
                var billDetail = new BillDetail() { Id = detailId++, Bill = billId, Item = detail.Item, Unit = detail.Unit, Quantity = detail.Quantity, Price = detail.Price, Discount = detail.Discount, Note = detail.Note, Ordinal = i + 1 };
                _context.BillDetails.AddRange(billDetail);
            }

            _context.SaveChanges();

            _context.Database.ExecuteSqlRaw("call calculate_strans_bill(" + billId + ");");
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
                    DB.DeleteStoreTransactionsForProcessing(_context, processing);
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
            if (DB.EditProcessingDateTime(_context, id, date, time, out errorMesssage))
                return Ok();

            return BadRequest(errorMesssage);
        }
    }
}
