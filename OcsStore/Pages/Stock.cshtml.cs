using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class StockModel : PageModel
    {
        private readonly ItemController _itemController;

        public StockModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
        }
    }
}
