using OcsStore.Models;

namespace OcsStore
{
    public class DBStock
    {
        public static StockView[] GetStocks(MyDbContext _context, DateTime date, sbyte itemGroupId, string materialIds)
        {
            date = Common.GetLocalDateWithoutTime(date); // Remove hour, minute...
            if (date >= DateTime.Today)
                return GetStocks(_context, itemGroupId, materialIds);

            int[] itemIds;
            if (itemGroupId == 3 && !string.IsNullOrEmpty(materialIds))
            {
                var materialIdArray = materialIds.Split(",").Select(int.Parse);
                itemIds = _context.ItemMaterials.Where(i => materialIdArray.Contains(i.Material)).Select(i => i.Item).Distinct().ToArray();
            }
            else
                itemIds = _context.Items.Where(i => i.Group == itemGroupId).Select(i => i.Id).ToArray();

            List<StockView> data = new List<StockView>();
            var itemStoreTranOrdinals = _context.StoreTransactions.Where(i => itemIds.Contains(i.Item) && i.Date <= date).GroupBy(e => new { e.Item }).Select(g => new { g.Key.Item, Ordinal = g.Max(e => e.Ordinal) }).ToArray();

            foreach (var r in itemStoreTranOrdinals)
            {
                var item = _context.Items.FirstOrDefault(i => i.Id == r.Item);
                var tran = _context.StoreTransactions.FirstOrDefault(i => i.Item == r.Item && i.Ordinal == r.Ordinal);
                var stockView = new StockView() { Item = r.Item, ItemName = item.Name, ItemGroup = item.Group, LastTransaction = tran.Id, Date = tran.Date, Soh = (decimal)tran.Soh, Ave = tran.Ave, Value = tran.Value, ValueK = tran.Value / 1000 };
                data.Add(stockView);
            }

            return data.ToArray();
        }

        public static StockView[] GetStocks(MyDbContext _context, sbyte itemGroupId, string materialIds)
        {
            if (itemGroupId == 3 && !string.IsNullOrEmpty(materialIds))
            {
                var materialIdArray = materialIds.Split(",").Select(int.Parse);
                var itemIds = _context.ItemMaterials.Where(i => materialIdArray.Contains(i.Material)).Select(i => i.Item).Distinct().ToList();
                return _context.StockViews.Where(i => i.ItemGroup == itemGroupId && itemIds.Contains(i.Item)).ToArray();
            }
            return _context.StockViews.Where(i => i.ItemGroup == itemGroupId).ToArray();
        }
    }
}
