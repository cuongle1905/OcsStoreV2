using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Controllers;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class UnitsModel : PageModel
    {
        private readonly ItemController _itemController;
        public Unit[] BaseUnits;

        public UnitsModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
            BaseUnits = _itemController.GetBaseUnits();
        }
    }
}
