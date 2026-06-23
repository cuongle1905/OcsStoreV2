using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OcsStore.Models;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace OcsStore.Controllers
{
    [Route("api/[controller]/[action]")]
    public class StockController: Controller
    {
        private MyDbContext _context;

        public StockController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult GetStocks(sbyte itemGroupId, string materialIds, DataSourceLoadOptions loadOptions)
        {
            IQueryable<StockView> data;
            if (itemGroupId == 3 && !string.IsNullOrEmpty(materialIds))
            {
                var materialIdArray = materialIds.Split(",").Select(int.Parse);
                var itemIds = _context.ItemMaterials.Where(i => materialIdArray.Contains(i.Material)).Select(i => i.Item).Distinct().ToList();
                data = _context.StockViews.Where(i => i.ItemGroup == itemGroupId && itemIds.Contains(i.Item) && (i.Lot == null || i.Soh > 0));
            }
            else
            {
                data = _context.StockViews.Where(i => i.ItemGroup == itemGroupId && (i.Lot == null || i.Soh > 0));
            }

            var result = DataSourceLoader.Load(data, loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult GetStockCard(int itemId, string lot, short year, DataSourceLoadOptions loadOptions)
        {
            IQueryable<StockCardView> data;
            if (string.IsNullOrEmpty(lot) || year <= 0)
                data = _context.StockCardViews.Where(i => i.Item == itemId);
            else
                data = _context.StockCardViews.Where(i => i.Item == itemId && i.Lot == lot && i.Year == year);

            var result = DataSourceLoader.Load(data, loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult CalculateItemSoh(int itemId)
        {
            CalculateStoreTransactionItem(itemId);
            return Ok();
        }

        [HttpPost]
        public IActionResult CalculateItemGroupSoh(int itemGroupId)
        {
            var itemIds = _context.Items.Where(i => i.Group == itemGroupId).Select(i => i.Id).ToArray();
            foreach (var itemId in itemIds)
            {
                CalculateStoreTransactionItem(itemId);
            }

            if (itemGroupId == 1)
            {
                var detailIdQuery = _context.StoreTransactions.Where(i => i.Type == 1).Select(i => i.DetailId);
                var missingReceivingIds = _context.ReceivingDetails.Where(i => !detailIdQuery.Contains(i.Id)).Select(i => i.Receiving).Distinct().ToArray();
                foreach(var receivingId in missingReceivingIds)
                {
                    _context.Database.ExecuteSqlRaw("call calculate_strans_receiving(" + receivingId + ");");
                }
            }
            else
            {
                var detailIdQuery = _context.StoreTransactions.Where(i => i.Type == 2).Select(i => i.DetailId);
                var missingProcessingIds = _context.ProcessingInputs.Where(i => !detailIdQuery.Contains(i.Id)).Select(i => i.Processing).Distinct().ToArray();
                foreach (var processingId in missingProcessingIds)
                {
                    _context.Database.ExecuteSqlRaw("call calculate_strans_processing(" + processingId + ");");
                }
            }

            return Ok();
        }

        private void CalculateStoreTransactionItem(int itemId)
        {
            _context.Database.ExecuteSqlRaw("call calculate_strans_item(" + itemId + ");");
        }


        [HttpPost]
        public IActionResult GetInventoryDetails(sbyte itemGroupId, DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.InventoryDetailViews.Where(i => i.ItemGroup == itemGroupId), loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult GetNewInventoryDetails(sbyte itemGroupId, DataSourceLoadOptions loadOptions)
        {
            var stocks = _context.StockViews.Where(i => i.ItemGroup == itemGroupId);
            List<InventoryDetailView> details = new List<InventoryDetailView>();

            foreach (var s in stocks)
            {
                var detail = new InventoryDetailView() { Selected = false, Item = s.Item, ItemGroup = s.ItemGroup, ItemName = s.ItemName, UseLot = s.UseLot ?? false, Lot = s.Lot, Year = (sbyte)(s.Year ?? DateTime.Today.Year % 100), Soh = s.Soh ?? 0 };
                details.Add(detail);
            }

            var result = DataSourceLoader.Load(details, loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult SaveInventory(DateTime date, string time, InventoryDetail[] details)
        {
            date = Common.GetLocalDateWithoutTime(date); // Remove hour, minute...
            int inventoryId;
            try
            {
                inventoryId = _context.Inventories.Max(i => i.Id) + 1;
            }
            catch
            {
                inventoryId = 1;
            }

            var inventory = new Inventory() { Id = inventoryId, Date = date, Time = time, UserCreated = Session.UserId(Request) };
            _context.Inventories.Add(inventory);

            int detailId;
            try
            {
                detailId = _context.InventoryDetails.Max(i => i.Id) + 1;
            }
            catch
            {
                detailId = 1;
            }

            foreach (var d in details)
            {
                var inventoryDetail = new InventoryDetail() { Id = detailId++, Inventory = inventoryId, Item = d.Item, Unit = d.Unit, Lot = d.Lot, Year = d.Year, Soh = d.Soh, Ave = d.Ave };
                _context.InventoryDetails.Add(inventoryDetail);
            }

            _context.SaveChanges();

            _context.Database.ExecuteSqlRaw("call calculate_strans_inventory(" + inventoryId + ");");

            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteInventory(int inventory)
        {
            _context.Database.ExecuteSqlRaw("call delete_inventory(" + inventory + ");");
            return Ok();
        }

        [HttpPost]
        public IActionResult GetMaterialSoh(int itemId)
        {
            var materialId = _context.ItemMaterials.FirstOrDefault(i => i.Item == itemId).Material;
            var soh = _context.StockViews.FirstOrDefault(i => i.Item == materialId).Soh ?? 0;
            return Ok(soh);
        }
    }
}
