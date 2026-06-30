using OcsStore.Models;

namespace OcsStore
{
    public class DBProcessing
    {
        public static void UpdateStoreTransactionForProcessingOutput(MyDbContext _context, int tranId, Processing p)
        {
            var ordinal = DB.GetNewStoreTransactionOrdinal(_context, p.Year, p.Date, p.Time, tranId);

            var totalValue = _context.StoreTransactions.Where(i => i.Type == StoreTransactionType.Processing && i.MainId == p.Id).Sum(i => i.Price * i.Quantity);
            var price = -totalValue / p.Quantity;

            var tran = new StoreTransaction() { Id = tranId, Date = p.Date, Time = p.Time, Type = StoreTransactionType.Processing, Store = p.Store, MainId = p.Id, Item = p.Item, Unit = p.Unit, Lot = p.Lot, Year = p.Year, Quantity = p.Quantity, Price = price, User = p.User, Ordinal = ordinal };

            _context.StoreTransactions.Add(tran);
            _context.SaveChanges();

            DB.UpdateStoreTransactions(_context, p.Store, p.Item, p.Unit, p.Lot, p.Year, ordinal);
        }

        public static void UpdateStoreTransactionForProcessingLotInput(MyDbContext _context, int tranId, Processing p, ProcessingInput pi, ProcessingLotInput pli)
        {
            var ordinal = DB.GetNewStoreTransactionOrdinal(_context, pli.Year, p.Date, p.Time, tranId);
            var tran = new StoreTransaction() { Id = tranId, Date = p.Date, Time = p.Time, Type = StoreTransactionType.Processing, Store = p.Store, MainId = p.Id, DetailId = pli.Id, Item = pi.Item, Unit = pi.Unit, Lot = pli.Lot, Year = pli.Year, Quantity = -pli.Quantity, User = p.User, Ordinal = ordinal };

            _context.StoreTransactions.Add(tran);
            _context.SaveChanges();

            DB.UpdateStoreTransactions(_context, p.Store, pi.Item, pi.Unit, pli.Lot, pli.Year, ordinal);
        }

        public static void DeleteStoreTransactionsForProcessing(MyDbContext _context, Processing processing)
        {
            DB.DeleteStoreTransaction(_context, StoreTransactionType.Processing, processing.Id, null);

            var detailIds = _context.ProcessingLotInputViews.Where(i => i.Processing == processing.Id).Select(i => i.Id).ToArray();
            foreach (var detailId in detailIds)
            {
                DB.DeleteStoreTransaction(_context, StoreTransactionType.Processing, processing.Id, detailId);
            }
        }

        public static void UpdateStoreTransactionDateTimesForProcessing(MyDbContext _context, Processing processing)
        {
            var outputStoreTransaction = _context.StoreTransactions.FirstOrDefault(i => i.Type == 2 && i.MainId == processing.Id && i.DetailId == null);
            if (outputStoreTransaction != null)
                DB.UpdateStoreTransactionDateTime(_context, outputStoreTransaction, processing.Date, processing.Time);

            var details = _context.ProcessingLotInputViews.Where(i => i.Processing == processing.Id).ToArray();
            foreach (var detail in details)
            {
                var inputStoreTransaction = _context.StoreTransactions.FirstOrDefault(i => i.Type == 2 && i.MainId == processing.Id && i.DetailId == detail.Id);
                if (inputStoreTransaction != null)
                    DB.UpdateStoreTransactionDateTime(_context, inputStoreTransaction, processing.Date, processing.Time);
            }
        }

        public static bool EditProcessingDateTime(MyDbContext _context, int id, DateTime date, string time, out string errorMessage)
        {
            var processing = _context.Processings.FirstOrDefault(i => i.Id == id);
            if (processing != null)
            {
                processing.Date = Common.GetLocalDateWithoutTime(date);
                processing.Time = time;
                _context.Database.BeginTransaction();
                _context.Processings.Update(processing);
                _context.SaveChanges();
                try
                {
                    UpdateStoreTransactionDateTimesForProcessing(_context, processing);
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
