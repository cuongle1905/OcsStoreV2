using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Controllers;
using OcsStore.Models;
using System.Globalization;

namespace OcsStore.Pages
{
    public class BillingDetailModel : PageModel
    {
        private BillingController _billingController;
        public List<Customer> Customers = new List<Customer>();

        public BillingDetailModel(BillingController billingController)
        {
            _billingController = billingController;
        }

        public void OnGet()
        {
            Customers = _billingController.GetCustomers();
        }
    }
}
