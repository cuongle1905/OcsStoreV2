using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class StockModel : PageModel
    {
        [FromQuery(Name = "itemGroup")]
        public short ItemGroupId { get; set; } = 1;

        private readonly ItemController _itemController;

        public StockModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
            if (ItemGroupId <= 0)
                ItemGroupId = 1;
        }
    }
}
