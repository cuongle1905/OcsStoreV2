using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class ProductProcessingModel : PageModel
    {
        private readonly ItemController _itemController;
        public List<Item> Items = new List<Item>();

        public ProductProcessingModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
            Items = _itemController.GetItems(Item.Product);

            var allItem = new Item() { Id = 0, Name = "   " };
            Items.Insert(0, allItem);
        }
    }
}
