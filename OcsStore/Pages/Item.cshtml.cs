using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Controllers;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class ItemModel : PageModel
    {
        [FromQuery(Name = "sourceurl")]
        public string SourceUrl { get; set; }

        [FromQuery(Name = "item")]
        public short ItemId { get; set; }
        public Item Item { get; set; }


        [FromQuery(Name = "itemGroup")]
        public sbyte ItemGroupId { get; set; }

        public ItemGroup[] ItemGroups { get; set; }

        public Unit[] Units { get; set; }

        private readonly ItemController _itemController;

        public ItemModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
            ItemGroups = _itemController.GetItemGroups();

            Units = _itemController.GetUnits();

            if (ItemId > 0)
                Item = _itemController.GetItem(ItemId);
            else
            {
                ItemId = 0;
                Item = new Item() { Id = 0, Name = "", Group = (ItemGroupId > 0 ? ItemGroupId : (sbyte)1), Unit = 1 };
            }
        }
    }
}
