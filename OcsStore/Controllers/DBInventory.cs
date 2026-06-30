using OcsStore.Models;

namespace OcsStore
{
    public class DBInventory
    {
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
