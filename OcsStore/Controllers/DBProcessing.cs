using OcsStore.Models;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace OcsStore
{
    public class DBProcessing
    {
        public static void UpdateStoreTransactionsForMissingProcessingDetails(MyDbContext _context)
        {
            var tranId = DB.GetNewId(_context, "store_transaction");
            var processingIdQuery = _context.StoreTransactions.Where(i => i.Type == StoreTransactionType.Processing && i.DetailId == null).Select(i => i.MainId);
            var missingProcessings = _context.Processings.Where(i => !processingIdQuery.Contains(i.Id)).ToArray();
            foreach (var processing in missingProcessings)
            {
                DBProcessing.UpdateStoreTransactionForProcessingOutput(_context, tranId++, processing);
            }

            var detailIdQuery = _context.StoreTransactions.Where(i => i.Type == StoreTransactionType.Processing).Select(i => i.DetailId);
            var missingProcessingInputs = _context.ProcessingInputs.Where(i => !detailIdQuery.Contains(i.Id)).ToArray();
            foreach (var processingInput in missingProcessingInputs)
            {
                var processing = _context.Processings.FirstOrDefault(i => i.Id == processingInput.Processing);
                DBProcessing.UpdateStoreTransactionForProcessingInput(_context, tranId++, processing, processingInput);
            }
        }

        public static void UpdateStoreTransactionForProcessingOutput(MyDbContext _context, int tranId, Processing p)
        {
            var ordinal = DB.GetNewStoreTransactionOrdinal(_context, p.Date, p.Time, tranId);

            var totalValue = _context.StoreTransactions.Where(i => i.Type == StoreTransactionType.Processing && i.MainId == p.Id).Sum(i => i.Price * i.Quantity);
            var price = -totalValue / p.Quantity;

            var tran = new StoreTransaction() { Id = tranId, Date = p.Date, Time = p.Time, Type = StoreTransactionType.Processing, Store = p.Store, MainId = p.Id, Item = p.Item, Unit = p.Unit, Quantity = p.Quantity, Price = price, User = p.User, Ordinal = ordinal };

            _context.StoreTransactions.Add(tran);
            _context.SaveChanges();

            DB.UpdateStoreTransactions(_context, p.Store, p.Item, p.Unit, ordinal);
        }

        public static void UpdateStoreTransactionForProcessingInput(MyDbContext _context, int tranId, Processing p, ProcessingInput pi)
        {
            var ordinal = DB.GetNewStoreTransactionOrdinal(_context, p.Date, p.Time, tranId);
            var tran = new StoreTransaction() { Id = tranId, Date = p.Date, Time = p.Time, Type = StoreTransactionType.Processing, Store = p.Store, MainId = p.Id, DetailId = pi.Id, Item = pi.Item, Unit = pi.Unit, Quantity = -pi.Quantity, User = p.User, Ordinal = ordinal };

            _context.StoreTransactions.Add(tran);
            _context.SaveChanges();

            DB.UpdateStoreTransactions(_context, p.Store, pi.Item, pi.Unit, ordinal);
        }

        public static void DeleteStoreTransactionsForProcessing(MyDbContext _context, Processing processing)
        {
            DB.DeleteStoreTransaction(_context, StoreTransactionType.Processing, processing.Id, null);

            var detailIds = _context.ProcessingInputViews.Where(i => i.Processing == processing.Id).Select(i => i.Id).ToArray();
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

            var details = _context.ProcessingInputViews.Where(i => i.Processing == processing.Id).ToArray();
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
