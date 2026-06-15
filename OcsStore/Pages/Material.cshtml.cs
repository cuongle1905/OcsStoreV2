using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Controllers;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class MaterialModel : PageModel
    {
        [FromQuery(Name = "item")]
        public int ItemId { get; set; } = 0;

        [FromQuery(Name = "group")]
        public short ItemGroupId { get; set; } = 2;

        public Item[] Items { get; set; }

        private readonly ItemController _itemController;

        public MaterialModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
            Items = _itemController.GetAllItems();

            if (ItemId > 0)
            {
                var item = _itemController.GetItem(ItemId);
                if (item != null)
                {
                    ItemGroupId = item.Group;
                }
            }
        }
    }
}
