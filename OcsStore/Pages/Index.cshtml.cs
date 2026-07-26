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
        private readonly StockController _stockController;
        private readonly SalesController _salesController;
        public decimal BillTotal { get; set; }
        public decimal CustomerDebtTotal { get; set; }
        public decimal Stock1Value { get; set; }
        public decimal Stock2Value { get; set; }
        public decimal RecevingTotalValue { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal TotalProfit { get; set; }

        public IndexModel(ReportController controller, StockController stockController, SalesController salesController)
        {
            _reportController = controller;
            _stockController = stockController;
            _salesController = salesController;
        }

        public void OnGet()
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

                BillTotal = _reportController.GetBillTotal();
                CustomerDebtTotal = _reportController.GetCustomerDebtTotal();
                Stock1Value = _reportController.GetStockValue(1);
                Stock2Value = _reportController.GetStockValue(2);
                RecevingTotalValue = _reportController.GetReceivingTotalValue();
                TotalExpense = _reportController.GetTotalExpense();
                TotalProfit = BillTotal - RecevingTotalValue - TotalExpense;
            }
        }
    }
}
