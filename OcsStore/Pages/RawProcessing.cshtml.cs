using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class RawProcessingModel : PageModel
    {
        private readonly ItemController _itemController;
        public List<Item> Items = new List<Item>();

        public RawProcessingModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
            Items = _itemController.GetItems(Item.Intermediate);

            var allItem = new Item() { Id = 0, Name = "   " };
            Items.Insert(0, allItem);
        }
    }
}
