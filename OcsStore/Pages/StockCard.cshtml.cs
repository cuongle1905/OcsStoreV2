using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class StockCardModel : PageModel
    {
        [FromQuery(Name = "item")]
        public int ItemId { get; set; }
        public ItemView Item;

        [FromQuery(Name = "lot")]
        public string Lot { get; set; }

        [FromQuery(Name = "year")]
        public int Year { get; set; }

        private readonly ItemController _itemController;

        public StockCardModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
            Item = _itemController.GetItem(ItemId);
        }
    }
}
