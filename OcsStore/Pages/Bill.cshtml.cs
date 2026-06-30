using OcsStore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OcsStore.Models;

namespace OcsStore.Pages
{
    public class BillModel : PageModel
    {
        [FromQuery(Name = "id")]
        public int BillId { get; set; }

        [FromQuery(Name = "processing")]
        public int ProcessingId { get; set; }

        public Bill Bill;
        public BillDetailView[] BillDetails = [];

        private ItemController _itemController;
        private SalesController _salesController;
        private ProcessingController _processingController;
        public Customer[] Customers;
        public SaleItemView[] SaleItems = [];

        public BillModel(ItemController controller, SalesController salesController, ProcessingController processingController)
        {
            _itemController = controller;
            _salesController = salesController;
            _processingController = processingController;
        }

        public void OnGet()
        {
            Customers = _salesController.GetCustomers();
            SaleItems = _itemController.GetSaleItems();
            if (BillId > 0)
            {
                Bill = _salesController.GetBill(BillId);
                BillDetails = _salesController.GetBillDetails(BillId);
            }
            else
            {
                Bill = new Bill() { Id = 0, Date = DateTime.Today, Time = DateTime.Now.ToString("HH:mm") };
            }

            if (ProcessingId > 0)
            {
                var p = _processingController.GetProcessingView(ProcessingId);
                if (p != null)
                {
                    var item = _itemController.GetItemView(p.Item);

                    var billDetail = new BillDetailView() { Item = p.Item, ItemName = item.Name, Unit = item.Unit, Quantity = p.Quantity / (decimal)item.BuExchange, Price = p.SalePrice ?? 0, Discount = 0 };
                    billDetail.Value = billDetail.Quantity * billDetail.Price;
                    BillDetails = [billDetail];
                }
            }
        }
    }
}
