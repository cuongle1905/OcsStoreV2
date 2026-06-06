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

        public List<ItemView> GetReceivingItems()
        {
            return _context.ItemViews.Where(i => i.IsInput == true).ToList();
        }

        public string GetItemName(int itemId)
        {
            return _context.Items.FirstOrDefault(i => i.Id == itemId).Name;
        }

        [HttpPost]
        public IActionResult GetUnits()
        {
            var result = _context.Units.ToArray();
            return Ok(result);
        }
    }
}
