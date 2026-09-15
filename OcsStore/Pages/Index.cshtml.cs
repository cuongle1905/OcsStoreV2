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
        private readonly ReportController _reportController;
        private readonly StockController _stockController;
        private readonly SalesController _salesController;
        public decimal BillTotal { get; set; }
        public decimal CustomerDebtTotal { get; set; }
        public decimal Stock1Value { get; set; }
        public decimal Stock2Value { get; set; }
        public decimal RecevingTotalValue { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal TotalProfit { get; set; }
        public string UserDefaultPage { get; set; }
        public List<UserPermissionView> UserPermissions { get; set; }
        public bool IsAdmin { get; set; }

        public IndexModel(UserController userController, ReportController controller, StockController stockController, SalesController salesController)
        {
            _userController = userController;
            _reportController = controller;
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

                BillTotal = _reportController.GetBillTotal();
                CustomerDebtTotal = _reportController.GetCustomerDebtTotal();
                Stock1Value = _reportController.GetStockValue(1);
                Stock2Value = _reportController.GetStockValue(2);
                RecevingTotalValue = _reportController.GetReceivingTotalValue();
                TotalExpense = _reportController.GetTotalExpense();
                TotalProfit = BillTotal - RecevingTotalValue - TotalExpense;

                UserPermissions = _userController.GetUserPermissions(Request);
            }
            return Page();
        }

        public bool UserHasPermission(short permission)
        {
            return IsAdmin || UserPermissions.FirstOrDefault(i => i.Permission == permission) != null;
        }
    }
}
