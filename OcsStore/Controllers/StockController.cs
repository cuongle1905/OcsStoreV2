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
        public IActionResult GetStockDataSource(sbyte itemGroupId, string materialIds, DataSourceLoadOptions loadOptions)
        {
            var data = GetStocks(itemGroupId, materialIds);
            var result = DataSourceLoader.Load(data, loadOptions);
            return Ok(result);
        }

        public IQueryable<StockView> GetStocks(sbyte itemGroupId, string materialIds)
        {
            IQueryable<StockView> data;
            if (itemGroupId == 3 && !string.IsNullOrEmpty(materialIds))
            {
                var materialIdArray = materialIds.Split(",").Select(int.Parse);
                var itemIds = _context.ItemMaterials.Where(i => materialIdArray.Contains(i.Material)).Select(i => i.Item).Distinct().ToList();
                data = _context.StockViews.Where(i => i.ItemGroup == itemGroupId && itemIds.Contains(i.Item));
            }
            else
            {
                data = _context.StockViews.Where(i => i.ItemGroup == itemGroupId);
            }
            return data;
        }

        [HttpPost]
        public IActionResult GetStockCard(int itemId, DataSourceLoadOptions loadOptions)
        {
            var data = _context.StockCardViews.Where(i => i.Item == itemId);
            var result = DataSourceLoader.Load(data, loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult CalculateItemSoh(int itemId)
        {
            DB.UpdateItemAllStoreTransactions(_context, 1, itemId, 1);
            return Ok();
        }

        [HttpPost]
        public IActionResult CalculateItemGroupSoh(int itemGroupId)
        {
            DBInventory.UpdateStoreTransactionsForMissingInventoryDetails(_context);

            if (itemGroupId == 1)
                DBReceiving.UpdateStoreTransactionsForMissingReceivingDetails(_context);
            else
            {
                DBProcessing.UpdateStoreTransactionsForMissingProcessingDetails(_context);

                if (itemGroupId == 3)
                    DBBilling.UpdateStoreTransactionsForMissingBillDetails(_context);
            }

            var itemIds = _context.Items.Where(i => i.Group == itemGroupId).Select(i => i.Id).ToArray();
            foreach (var itemId in itemIds)
            {
                DB.UpdateItemAllStoreTransactions(_context, 1, itemId, 1);
            }

            return Ok();
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
            var stocks = GetStocks(itemGroupId, null).ToArray();
            List<InventoryDetailView> details = new List<InventoryDetailView>();

            foreach (var s in stocks)
            {
                var newDetail = new InventoryDetailView() { Selected = false, Item = s.Item, ItemGroup = s.ItemGroup, ItemName = s.ItemName, Soh = s.Soh };
                details.Add(newDetail);
            }

            var result = DataSourceLoader.Load(details, loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult SaveInventory(DateTime date, string time, InventoryDetail[] details)
        {
            date = Common.GetLocalDateWithoutTime(date); // Remove hour, minute...

            var dbTran = _context.Database.BeginTransaction();

            var inventoryId = DB.GetNewId(_context, "inventory");
            var inventory = new Inventory() { Id = inventoryId, Date = date, Time = time, UserCreated = Session.UserId(Request) };
            _context.Inventories.Add(inventory);

            var detailId = DB.GetNewId(_context, "inventory_detail");
            foreach (var d in details)
            {
                d.Id = detailId++;
                d.Inventory = inventoryId;
                d.Unit = 1;
                _context.InventoryDetails.Add(d);
            }
            _context.SaveChanges();

            DBInventory.UpdateStoreTransactionsForInventory(_context, inventory, details);

            dbTran.Commit();

            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteInventory(int inventory)
        {
            var inventoryData = _context.Inventories.FirstOrDefault(i => i.Id == inventory);
            if (inventoryData != null)
            {
                _context.Database.BeginTransaction();
                try
                {
                    DBInventory.DeleteStoreTransactionsForInventory(_context, inventoryData);
                    _context.Inventories.Remove(inventoryData);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    _context.Database.RollbackTransaction();
                    return BadRequest(ex.Message);
                }
                _context.Database.CommitTransaction();
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult GetMaterialSoh(int itemId)
        {
            var materialId = _context.ItemMaterials.FirstOrDefault(i => i.Item == itemId).Material;
            var soh = _context.StockViews.FirstOrDefault(i => i.Item == materialId).Soh;
            return Ok(soh);
        }

        [HttpPost]
        public IActionResult EditDateTime(int id, DateTime date, string time)
        {
            var tran = _context.StoreTransactions.FirstOrDefault(i => i.Id == id);
            if (tran != null)
            {
                string errorMesssage;
                if (tran.Type == StoreTransactionType.Receiving)
                {
                    if (!DBReceiving.EditReceivingDateTime(_context, tran.MainId, date, time, out errorMesssage))
                        return BadRequest(errorMesssage);
                }
                else if (tran.Type == StoreTransactionType.Processing)
                {
                    if (!DBProcessing.EditProcessingDateTime(_context, tran.MainId, date, time, out errorMesssage))
                        return BadRequest(errorMesssage);
                }
                else if (tran.Type == StoreTransactionType.Billing)
                {
                    if (!DBBilling.EditBillDateTime(_context, tran.MainId, date, time, out errorMesssage))
                        return BadRequest(errorMesssage);
                }
                else if (tran.Type == StoreTransactionType.Inventory)
                {
                    if (!DBInventory.EditInventoryDateTime(_context, tran.MainId, date, time, out errorMesssage))
                        return BadRequest(errorMesssage);
                }
            }
            return Ok();
        }

        public void RemoveInvalidStoreTransactions()
        {
            _context.Database.ExecuteSqlRaw("call delete_invalid_store_transactions();");
        }

        public void UpdateAllStoreTransactions()
        {
            DB.UpdateAllStoreTransactions(_context);
        }
    }
}
