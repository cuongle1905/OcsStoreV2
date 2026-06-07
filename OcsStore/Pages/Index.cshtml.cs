using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcsStore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ReportController _reportController;
        public decimal BillTotal { get; set; }
        public decimal CustomerDebtTotal { get; set; }
        public decimal RecevingItemTotalValue { get; set; }
        public decimal ProcessingItemTotalValue { get; set; }
        public decimal RecevingTotalValue { get; set; }
        public decimal TotalProfit { get; set; }

        public IndexModel(ReportController controller)
        {
            _reportController = controller;
        }

        public void OnGet()
        {
            if (!string.IsNullOrEmpty(Session.Token(Request)))
            {
                BillTotal = _reportController.GetBillTotal();
                CustomerDebtTotal = _reportController.GetCustomerDebtTotal();
                RecevingItemTotalValue = _reportController.GetReceivingItemTotalValue();
                ProcessingItemTotalValue = _reportController.GetProcessingItemTotalValue();
                RecevingTotalValue = _reportController.GetReceivingTotalValue();
                TotalProfit = _reportController.GetTotalProfit();
            }
        }
    }
}
