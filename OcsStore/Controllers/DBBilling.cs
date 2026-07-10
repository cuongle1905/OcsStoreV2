using OcsStore.Models;

namespace OcsStore
{
    public class DBBilling
    {
        public static void UpdateStoreTransactionsForMissingBillDetails(MyDbContext _context)
        {
            var tranId = DB.GetNewId(_context, "store_transaction");
            var detailIdQuery = _context.StoreTransactions.Where(i => i.Type == StoreTransactionType.Billing).Select(i => i.DetailId);
            var missingBillDetails = _context.BillDetails.Where(i => !detailIdQuery.Contains(i.Id)).ToArray();
            foreach (var billDetail in missingBillDetails)
            {
                var bill = _context.Bills.FirstOrDefault(i => i.Id == billDetail.Bill);
                UpdateStoreTransactionForBillDetail(_context, tranId++, bill, billDetail);
            }
        }

        public static void UpdateStoreTransactionForBillDetail(MyDbContext _context, int tranId, Bill b, BillDetail d)
        {
            var ordinal = DB.GetNewStoreTransactionOrdinal(_context, b.Date, b.Time, tranId);
            var buExchange = (decimal)_context.Units.FirstOrDefault(i => i.Id == d.Unit).BuExchange;

            var tran = new StoreTransaction() { Id = tranId, Date = b.Date, Time = b.Time, Type = StoreTransactionType.Billing, Store = b.Store, MainId = b.Id, DetailId = d.Id, Item = d.Item, Unit = 1, Quantity = -d.Quantity * buExchange, Price = d.Price / buExchange, User = b.UserCreated, Ordinal = ordinal };

            _context.StoreTransactions.Add(tran);
            _context.SaveChanges();

            DB.UpdateStoreTransactions(_context, b.Store, d.Item, d.Unit, ordinal);
        }

        public static void DeleteStoreTransactionsForBill(MyDbContext _context, Bill bill)
        {
            DB.DeleteStoreTransaction(_context, StoreTransactionType.Billing, bill.Id, null);

            var detailIds = _context.BillDetailViews.Where(i => i.Bill == bill.Id).Select(i => i.Id).ToArray();
            foreach (var detailId in detailIds)
            {
                DB.DeleteStoreTransaction(_context, StoreTransactionType.Billing, bill.Id, detailId);
            }
        }

        public static void UpdateStoreTransactionDateTimesForBill(MyDbContext _context, Bill bill)
        {
            DB.UpdateStoreTransactionDateTime(_context, StoreTransactionType.Billing, bill.Id, null, bill.Date, bill.Time);

            var detailIds = _context.BillDetailViews.Where(i => i.Bill == bill.Id).Select(i => i.Id).ToArray();
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
