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
            return Ok();
        }
    }
}
