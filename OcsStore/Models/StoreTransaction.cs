using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class StoreTransaction
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }

    public sbyte Type { get; set; }

    public short Store { get; set; }

    public int? MainId { get; set; }

    public int? DetailId { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public string Lot { get; set; }

    public sbyte Year { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal? Soh { get; set; }

    public decimal? Value { get; set; }

    public decimal? Ave { get; set; }

    public decimal? LotSoh { get; set; }

    public decimal? LotValue { get; set; }

    public decimal? LotAve { get; set; }

    public short User { get; set; }

    public long Ordinal { get; set; }

    public virtual Item ItemNavigation { get; set; }

    public virtual ICollection<LastStoreTransaction> LastStoreTransactions { get; set; } = new List<LastStoreTransaction>();

    public virtual Store StoreNavigation { get; set; }

    public virtual Unit UnitNavigation { get; set; }

    public virtual User UserNavigation { get; set; }
}
