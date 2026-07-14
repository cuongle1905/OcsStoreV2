using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class SalesModel : PageModel
    {
        private SalesController _salesController;
        public List<Customer> Customers = new List<Customer>();

        public SalesModel(SalesController salesController)
        {
            _salesController = salesController;
        }

        public void OnGet()
        {
            Customers = _salesController.GetCustomerList();

            var allCustomer = new Customer() { Id = 0, Name = "   " };
            Customers.Insert(0, allCustomer);
        }
    }
}
