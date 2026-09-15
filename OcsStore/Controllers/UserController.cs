using Microsoft.AspNetCore.Mvc;
using OcsStore.Models;

namespace OcsStore.Controllers
{
    [Route("api/[controller]/[action]")]
    public class UserController : Controller
    {
        private MyDbContext _context;
        public UserController(MyDbContext context)
        {
            _context = context;
        }

        public List<UserPermissionView> GetUserPermissions(HttpRequest request)
        {
            var userId = Session.UserId(request);
            var data = _context.UserPermissionViews.Where(i => i.UserId == userId).ToList();
            return data;
        }

        public string GetDefaultPage(HttpRequest request)
        {
            var userId = Session.UserId(request);
            var data = _context.UserPermissionViews.FirstOrDefault(i => i.UserId == userId && i.Page != null);
            if (data != null)
                return data.Page;

            return null;
        }
    }
}
