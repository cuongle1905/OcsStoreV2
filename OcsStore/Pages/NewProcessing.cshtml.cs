using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;
using System.Diagnostics;
using Microsoft.VisualBasic;

namespace OcsStore.Pages
{
    public class NewProcessingModel : PageModel
    {
        private readonly ItemController _itemController;

        [FromQuery(Name = "sourceurl")]
        public string SourceUrl { get; set; }

        [FromQuery(Name = "item")]
        public short ItemId { get; set; }
        public ItemView Item { get; set; }

        public NewProcessingModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
            if (ItemId > 0)
                Item = _itemController.GetItem(ItemId);
        }
    }
}
