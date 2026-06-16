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

        [FromQuery(Name = "item")]
        private short ItemId { get; set; }
        public ItemView Item { get; set; }

        [FromQuery(Name = "itemgroup")]
        public short ItemGroupId { get; set; }

        public NewProcessingModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
            if (ItemId > 0)
                Item = _itemController.GetItemView(ItemId);
            else
            {
                if (ItemGroupId <= 1)
                    ItemGroupId = 2;

                Item = _itemController.GetItemViewOfGroup(ItemGroupId);
            }
        }
    }
}
