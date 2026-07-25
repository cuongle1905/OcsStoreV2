using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;
using System.Diagnostics;
using Microsoft.VisualBasic;

namespace OcsStore.Pages
{
    public class NewProductProcessingModel : PageModel
    {
        private readonly ItemController _itemController;

        public List<Item> Items = new List<Item>();

        public NewProductProcessingModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
            Items = _itemController.GetItems(3);
        }
    }
}
