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

        public ItemView GetItem(int itemId)
        {
            return _context.ItemViews.FirstOrDefault(i => i.Id == itemId);
        }

        [HttpPost]
        public IActionResult GetUnits()
        {
            var result = _context.Units.ToArray();
            return Ok(result);
        }
    }
}
