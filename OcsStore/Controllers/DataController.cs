using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OcsStore.Models;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OcsStore.Controllers
{
    [Route("api/[controller]/[action]")]
    public class DataController : Controller
    {
        private MyDbContext _context;

        public DataController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult GetUserManagementViews(DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.UserManagementViews, loadOptions);
            return Ok(result);
        }

        private void SaveUser(User user)
        {
            if (user.Id == 0)
            {
                try
                {
                    user.Id = (short)(_context.Users.Max(i => i.Id) + 1);
                }
                catch
                {
                    user.Id = 1;
                }

                _context.Users.Add(user);
            }
            else
            {
                _context.Users.Update(user);
            }
            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult SaveUsers(User[] data)
        {
            foreach (User user in data)
            {
                SaveUser(user);
            }
            return Ok();
        }
    }
}
