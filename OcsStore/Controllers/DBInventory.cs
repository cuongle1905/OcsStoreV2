using OcsStore.Models;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace OcsStore
{
    public class DBInventory
    {
        public static void UpdateStoreTransactionsForMissingInventoryDetails(MyDbContext _context)
        {
            var tranId = DB.GetNewId(_context, "store_transaction");
            var detailIdQuery = _context.StoreTransactions.Where(i => i.Type == StoreTransactionType.Inventory).Select(i => i.DetailId);
            var missingInventoryDetails = _context.InventoryDetails.Where(i => !detailIdQuery.Contains(i.Id)).ToArray();
            foreach (var inventoryDetail in missingInventoryDetails)
            {
                var inventory = _context.Inventories.FirstOrDefault(i => i.Id == inventoryDetail.Inventory);
                DBInventory.UpdateStoreTransactionForInventoryDetail(_context, tranId++, inventory, inventoryDetail);
            }
        }

        public static void UpdateStoreTransactionsForInventory(MyDbContext _context, Inventory r, InventoryDetail[] details)
        {
            var tranId = DB.GetNewId(_context, "store_transaction");

            foreach (InventoryDetail d in details)
            {
                UpdateStoreTransactionForInventoryDetail(_context, tranId++, r, d);
            }
        }

        public static void UpdateStoreTransactionForInventoryDetail(MyDbContext _context, int tranId, Inventory r, InventoryDetail d)
        {
            var ordinal = DB.GetNewStoreTransactionOrdinal(_context, r.Date, r.Time, tranId);
            var tran = new StoreTransaction() { Id = tranId, Date = r.Date, Time = r.Time, Type = StoreTransactionType.Inventory, Store = r.Store, MainId = r.Id, DetailId = d.Id, Item = d.Item, Unit = d.Unit, Soh = d.Soh, Ave = d.Ave, User = r.UserCreated, Ordinal = ordinal };

            _context.StoreTransactions.Add(tran);
            _context.SaveChanges();

            DB.UpdateStoreTransactions(_context, r.Store, d.Item, d.Unit, ordinal, true);
        }

        public static void DeleteStoreTransactionsForInventory(MyDbContext _context, Inventory inventory)
        {
            DB.DeleteStoreTransaction(_context, StoreTransactionType.Inventory, inventory.Id, null);

            var detailIds = _context.InventoryDetailViews.Where(i => i.Inventory == inventory.Id).Select(i => i.Id).ToArray();
            foreach (var detailId in detailIds)
            {
                DB.DeleteStoreTransaction(_context, StoreTransactionType.Inventory, inventory.Id, detailId);
            }
        }

        public static void UpdateStoreTransactionDateTimesForInventory(MyDbContext _context, Inventory inventory)
        {
            DB.UpdateStoreTransactionDateTime(_context, StoreTransactionType.Inventory, inventory.Id, null, inventory.Date, inventory.Time);

            var detailIds = _context.InventoryDetails.Where(i => i.Inventory == inventory.Id).Select(i => i.Id).ToArray();
            DB.UpdateStoreTransactionDateTimes(_context, StoreTransactionType.Inventory, inventory.Id, detailIds, inventory.Date, inventory.Time);
        }

        public static bool EditInventoryDateTime(MyDbContext _context, int id, DateTime date, string time, out string errorMessage)
        {
            var inventory = _context.Inventories.FirstOrDefault(i => i.Id == id);
            if (inventory != null)
            {
                inventory.Date = Common.GetLocalDateWithoutTime(date);
                inventory.Time = time;
                _context.Database.BeginTransaction();
                _context.Inventories.Update(inventory);
                _context.SaveChanges();
                try
                {
                    UpdateStoreTransactionDateTimesForInventory(_context, inventory);
                }
                catch (Exception ex)
                {
                    _context.Database.RollbackTransaction();
                    errorMessage = ex.Message;
                    return false;
                }
                _context.Database.CommitTransaction();
            }
            errorMessage = null;
            return true;
        }

    }
}
