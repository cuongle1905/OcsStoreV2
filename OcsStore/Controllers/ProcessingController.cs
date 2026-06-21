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
        public IActionResult GetProcessings(sbyte itemGroup, DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.ProcessingViews.Where(i => i.ItemGroup == itemGroup), loadOptions);
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
            processing.Date = Common.GetLocalDateWithoutTime(processing.Date); // Remove hour, minute...
            int processingId = GetNewProcessingId();

            var item = _context.ItemViews.FirstOrDefault(i => i.Id == processing.Item);
            var yy = (sbyte)(processing.Date.Year % 100);
            var newProcessing = new Processing() { Id = processingId, Year = yy, Item = processing.Item, Quantity = processing.Quantity, Date = processing.Date, Time = processing.Time, User = Session.UserId(Request) };

            _context.Processings.Add(newProcessing);

            var materialId = _context.ItemMaterials.FirstOrDefault(i => i.Item == processing.Item).Material;

            int inputId = GetNewProcessingInputId();

            var processingInput = new ProcessingInput() { Id = inputId, Processing = processingId, Item = materialId, Quantity = materialQuantity };
            _context.ProcessingInputs.Add(processingInput);

            int lotInputId = GetNewProcessingLotInputId();
            var processingLotInput = new ProcessingLotInput() { Id = lotInputId++, Input = inputId, Lot = null, Year = yy, Quantity = materialQuantity };
            _context.ProcessingLotInputs.Add(processingLotInput);

            _context.SaveChanges();

            _context.Database.ExecuteSqlRaw("call calculate_strans_processing(" + processingId + ");");

            return Ok();
        }

        [HttpPost]
        public IActionResult Save(Processing processing, ProcessingLotInputView[] details, bool createBill, decimal salePrice, Customer customer, bool debit)
        {
            processing.Date = Common.GetLocalDateWithoutTime(processing.Date); // Remove hour, minute...
            int processingId;
            try
            {
                processingId = _context.Processings.Max(i => i.Id) + 1;
            }
            catch
            {
                processingId = 1;
            }

            var item = _context.ItemViews.FirstOrDefault(i => i.Id == processing.Item);
            var newProcessing = new Processing() { Id = processingId, Year = (sbyte)(processing.Date.Year % 100), Item = processing.Item, Quantity = processing.Quantity, Date = processing.Date, Time = processing.Time, User = Session.UserId(Request) };

            if (item.UseLot)
                newProcessing.Lot = processing.Date.ToString("ddMM");

            _context.Processings.Add(newProcessing);

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

            if (createBill)
            {
                var billDetail = new BillDetail() { Item = processing.Item, Unit = processing.Unit, Quantity = processing.Quantity, Price = salePrice };
                BillDetail[] billDetails = { billDetail };
                CreateBill(processing.Date, processing.Time, billDetails.ToArray(), customer, debit);
            }

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
    }
}
