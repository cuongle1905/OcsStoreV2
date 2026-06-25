using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class NewBillModel : PageModel
    {
        private ItemController _itemController;
        public SaleItemView[] SaleItems = [];

        public NewBillModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
            SaleItems = _itemController.GetSaleItems();
        }
    }
}
