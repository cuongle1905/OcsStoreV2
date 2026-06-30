using OcsStore.Models;

namespace OcsStore
{
    public class DBBilling
    {
        public static void UpdateStoreTransactionForBillLotDetail(MyDbContext _context, int tranId, Bill b, BillDetail bd, BillLotDetail d)
        {
            /* insert into store_transaction (id, date, time, type, store, main_id, detail_id, item, unit, `year`, quantity, `user`, ordinal)
                values (tranId, v_date, v_time, 3, storeId, billId, detailId, itemId, unitId, yy, -v_quantity, userId, v_ordinal); */

            var ordinal = DB.GetNewStoreTransactionOrdinal(_context, d.Year, b.Date, b.Time, tranId);
            var tran = new StoreTransaction() { Id = tranId, Date = b.Date, Time = b.Time, Type = StoreTransactionType.Billing, Store = b.Store, MainId = b.Id, DetailId = d.Id, Item = bd.Item, Unit = 1, Lot = d.Lot, Year = d.Year, Quantity = -d.Quantity, User = b.UserCreated, Ordinal = ordinal };

            _context.StoreTransactions.Add(tran);
            _context.SaveChanges();

            DB.UpdateStoreTransactions(_context, b.Store, bd.Item, bd.Unit, d.Lot, d.Year, ordinal);
        }

        public static void DeleteStoreTransactionsForBill(MyDbContext _context, Bill bill)
        {
            DB.DeleteStoreTransaction(_context, StoreTransactionType.Billing, bill.Id, null);

            var detailIds = _context.BillLotDetailViews.Where(i => i.Bill == bill.Id).Select(i => i.Id).ToArray();
            foreach (var detailId in detailIds)
            {
                DB.DeleteStoreTransaction(_context, StoreTransactionType.Billing, bill.Id, detailId);
            }
        }

        public static void UpdateStoreTransactionDateTimesForBill(MyDbContext _context, Bill bill)
        {
            DB.UpdateStoreTransactionDateTime(_context, StoreTransactionType.Billing, bill.Id, null, bill.Date, bill.Time);

            var detailIds = _context.BillLotDetailViews.Where(i => i.Bill == bill.Id).Select(i => i.Id).ToArray();
            DB.UpdateStoreTransactionDateTimes(_context, StoreTransactionType.Billing, bill.Id, detailIds, bill.Date, bill.Time);
        }

        public static bool EditBillDateTime(MyDbContext _context, int id, DateTime date, string time, out string errorMessage)
        {
            var bill = _context.Bills.FirstOrDefault(i => i.Id == id);
            if (bill != null)
            {
                bill.Date = Common.GetLocalDateWithoutTime(date);
                bill.Time = time;
                _context.Database.BeginTransaction();
                _context.Bills.Update(bill);
                _context.SaveChanges();
                try
                {
                    UpdateStoreTransactionDateTimesForBill(_context, bill);
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
