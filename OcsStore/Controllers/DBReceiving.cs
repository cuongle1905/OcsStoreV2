using OcsStore.Models;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace OcsStore
{
    public class DBReceiving
    {
        public static void UpdateStoreTransactionsForMissingReceivingDetails(MyDbContext _context)
        {
            var tranId = DB.GetNewId(_context, "store_transaction");
            var detailIdQuery = _context.StoreTransactions.Where(i => i.Type == StoreTransactionType.Receiving).Select(i => i.DetailId);
            var missingReceivingDetails = _context.ReceivingDetails.Where(i => !detailIdQuery.Contains(i.Id)).ToArray();
            foreach (var receivingDetail in missingReceivingDetails)
            {
                var receiving = _context.Receivings.FirstOrDefault(i => i.Id == receivingDetail.Receiving);
                UpdateStoreTransactionForReceivingDetail(_context, tranId++, receiving, receivingDetail);
            }
        }

        public static void UpdateStoreTransactionsForReceiving(MyDbContext _context, Receiving r, List<ReceivingDetail> details)
        {
            var tranId = DB.GetNewId(_context, "store_transaction");
            foreach (ReceivingDetail d in details)
            {
                UpdateStoreTransactionForReceivingDetail(_context, tranId++, r, d);
            }
        }

        public static void UpdateStoreTransactionForReceivingDetail(MyDbContext _context, int tranId, Receiving r, ReceivingDetail d)
        {
            /* insert into store_transaction (id, date, time, type, store, main_id, detail_id, item, unit, `year`, quantity, price, `user`, ordinal)
                values (tranId, v_date, v_time, 1, storeId, receivingId, detailId, itemId, unitId, yy, v_quantity, v_price, userId, v_ordinal); */

            var ordinal = DB.GetNewTransactionOrdinal(_context, r.Date, r.Time, tranId);
            var tran = new StoreTransaction() { Id = tranId, Date = r.Date, Time = r.Time, Type = 1, Store = r.Store, MainId = r.Id, DetailId = d.Id, Item = d.Item, Unit = d.Unit, Quantity = d.Quantity, Price = d.Price, User = r.User, Ordinal = ordinal };

            _context.StoreTransactions.Add(tran);
            _context.SaveChanges();

            DB.UpdateStoreTransactions(_context, r.Store, d.Item, d.Unit, ordinal, true);
        }

        public static void DeleteStoreTransactionsForReceivingDetail(MyDbContext _context, ReceivingDetail detail)
        {
            DB.DeleteStoreTransaction(_context, 1, detail.Receiving, detail.Id);
        }

        public static void UpdateStoreTransactionDateTimesForReceiving(MyDbContext _context, Receiving receiving)
        {
            var detailIds = _context.ReceivingDetails.Where(i => i.Receiving == receiving.Id).Select(i => i.Id).ToArray();
            DB.UpdateStoreTransactionDateTimes(_context, StoreTransactionType.Processing, receiving.Id, detailIds, receiving.Date, receiving.Time);
        }

        public static bool EditReceivingDateTime(MyDbContext _context, int id, DateTime date, string time, out string errorMessage)
        {
            var receiving = _context.Receivings.FirstOrDefault(i => i.Id == id);
            if (receiving != null)
            {
                receiving.Date = Common.GetLocalDateWithoutTime(date);
                receiving.Time = time;
                _context.Database.BeginTransaction();
                _context.Receivings.Update(receiving);
                _context.SaveChanges();
                try
                {
                    UpdateStoreTransactionDateTimesForReceiving(_context, receiving);
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
