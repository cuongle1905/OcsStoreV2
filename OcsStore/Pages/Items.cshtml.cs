using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Controllers;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class ItemsModel : PageModel
    {
        private readonly ItemController _itemController;
        public Unit[] Units;

        public ItemsModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
            Units = _itemController.GetUnits();
        }
    }
}
