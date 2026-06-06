using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class NewReceivingModel : PageModel
    {
        private readonly ItemController _itemController;
        public List<ReceivingDetailView> Details = new List<ReceivingDetailView>();

        public NewReceivingModel(ItemController controller)
        {
            _itemController = controller;
        }

        public void OnGet()
        {
            Details = CreateNewDetails();
        }

        public List<ReceivingDetailView> CreateNewDetails()
        {
            var details = new List<ReceivingDetailView>();
            var items = _itemController.GetReceivingItems();
            foreach (var item in items)
            {
                var detail = new ReceivingDetailView { Item = item.Id, ItemName = item.Name, Unit = item.Unit, UnitName = item.UnitName };
                details.Add(detail);
            }
            return details;
        }
    }
}
