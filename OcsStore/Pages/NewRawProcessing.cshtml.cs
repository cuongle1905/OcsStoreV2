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

        [FromQuery(Name = "item")]
        short ItemId { get; set; }

        Item[] Items;

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
