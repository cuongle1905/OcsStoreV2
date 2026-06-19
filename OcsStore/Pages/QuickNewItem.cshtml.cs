using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Controllers;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class QuickNewItemModel : PageModel
    {
        private ItemController _itemController;
        public Item[] Materials;
        public int DefaultMaterial1, DefaultMaterial2;

        public QuickNewItemModel(ItemController itemController)
        {
            _itemController = itemController;
        }

        public void OnGet()
        {
            Materials = _itemController.GetItems(2);
            DefaultMaterial1 = _itemController.FirstMaterialIdToCreateItem();
            DefaultMaterial2 = _itemController.SecondMaterialIdToCreateItem();
        }
    }
}
