using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using OcsStore.Models;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace OcsStore.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ItemController: Controller
    {
        private MyDbContext _context;

        public ItemController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult GetItemViews(int groupId, DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.ItemViews.Where(i => i.Group == groupId), loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult GetItems(int groupId, DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.ItemViews.Where(i => i.Group == groupId), loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult GetItemManagementViews(int groupId, DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.ItemManagementViews.Where(i => i.Group == groupId), loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult GetNormalUnits(DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.UnitManagementViews.Where(i => i.Id != i.BaseUnit), loadOptions);
            return Ok(result);
        }

        public Unit[] GetBaseUnits()
        {
            return _context.Units.Where(i => i.Id == i.BaseUnit).ToArray();
        }

        public List<ItemView> GetReceivingItems()
        {
            return _context.ItemViews.Where(i => i.ItemType == Item.Receving).ToList();
        }

        public ItemView GetItemView(int itemId)
        {
            return _context.ItemViews.FirstOrDefault(i => i.Id == itemId);
        }

        public Item GetItem(int itemId)
        {
            return _context.Items.FirstOrDefault(i => i.Id == itemId);
        }

        public Unit[] GetUnits()
        {
            return _context.Units.ToArray();
        }

        public ItemGroup[] GetItemGroups()
        {
            return _context.ItemGroups.ToArray();
        }

        [HttpPost]
        public IActionResult Save(Item item)
        {
            SaveItem(item);
            return Ok();
        }

        private void SaveItem(Item item)
        {
            if (item.Id == 0)
            {
                try
                {
                    item.Id = _context.Items.Max(i => i.Id) + 1;
                }
                catch
                {
                    item.Id = 1;
                }
                if (string.IsNullOrEmpty(item.FullName))
                    item.FullName = item.Name;

                if (string.IsNullOrEmpty(item.Code))
                    item.Code = item.Name;

                _context.Items.Add(item);
            }
            else
            {
                _context.Items.Update(item);
            }
            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult SaveItems(Item[] data)
        {
            foreach (Item item in data)
            {
                SaveItem(item);
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (_context.ReceivingDetails.FirstOrDefault(i => i.Item == id) == null
                && _context.ProcessingInputs.FirstOrDefault(i => i.Item == id) == null)
            {
                var item = _context.Items.FirstOrDefault(i => i.Id == id);
                if (item != null)
                {
                    _context.Items.Remove(item);
                    _context.SaveChanges();
                }
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteUnit(int id)
        {
            if (_context.ReceivingDetails.FirstOrDefault(i => i.Unit == id) == null
                && _context.ProcessingInputs.FirstOrDefault(i => i.Unit == id) == null
                && _context.BillDetails.FirstOrDefault(i => i.Unit == id) == null)
            {
                var unit = _context.Units.FirstOrDefault(i => i.Id == id);
                if (unit != null)
                {
                    _context.Units.Remove(unit);
                    _context.SaveChanges();
                }
            }
            return Ok();
        }

        private void SaveUnit(Unit unit)
        {
            if (unit.Id == 0)
            {
                try
                {
                    unit.Id = (short)(_context.Units.Max(i => i.Id) + 1);
                }
                catch
                {
                    unit.Id = 1;
                }
                if (string.IsNullOrEmpty(unit.FullName))
                    unit.FullName = unit.Name;

                _context.Units.Add(unit);
            }
            else
            {
                _context.Units.Update(unit);
            }
            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult SaveUnits(Unit[] data)
        {
            foreach (Unit unit in data)
            {
                SaveUnit(unit);
            }
            return Ok();
        }
    }
}
