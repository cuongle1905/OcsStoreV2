using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Controllers;
using OcsStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcsStore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserController _userController;
        private readonly StockController _stockController;
        private readonly SalesController _salesController;
        public string UserDefaultPage { get; set; }
        public List<UserPermissionView> UserPermissions { get; set; }
        public bool IsAdmin { get; set; }
        public List<IdName> ReportMonths = new List<IdName>();

        public IndexModel(UserController userController, StockController stockController, SalesController salesController)
        {
            _userController = userController;
            _stockController = stockController;
            _salesController = salesController;
        }

        public IActionResult OnGet()
        {
            if (!string.IsNullOrEmpty(Session.Token(Request)))
            {
                try
                {
                    _stockController.RemoveInvalidStoreTransactions();
                    _stockController.UpdateAllStoreTransactions();
                }
                catch
                {
                }
                _salesController.UpdateMissingCustomerTransactions();

                IsAdmin = Session.IsAdmin(Request);

                if (!IsAdmin)
                {
                    UserDefaultPage = _userController.GetDefaultPage(Request);
                    return Redirect("/" + UserDefaultPage);
                }

                UserPermissions = _userController.GetUserPermissions(Request);

                ReportMonths = _salesController.GetReportMonths();
            }
            return Page();
        }

        public bool UserHasPermission(short permission)
        {
            return IsAdmin || UserPermissions.FirstOrDefault(i => i.Permission == permission) != null;
        }
    }
}
