using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;
using System.Diagnostics;
using Microsoft.VisualBasic;

namespace OcsStore.Pages
{
    public class NewRawProcessingModel : PageModel
    {
        private readonly ItemController _itemController;

        public List<Item> Items = new List<Item>();

        public NewRawProcessingModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
            Items = _itemController.GetItems(2);
        }
    }
}
