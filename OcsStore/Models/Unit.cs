using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class Unit
{
    public short Id { get; set; }

    public string Name { get; set; }

    public string FullName { get; set; }

    public short BaseUnit { get; set; }

    public double BuExchange { get; set; }

    public string Note { get; set; }

    public virtual ICollection<LastStoreTransaction> LastStoreTransactions { get; set; } = new List<LastStoreTransaction>();

    public virtual ICollection<ProcessingDetail> ProcessingDetails { get; set; } = new List<ProcessingDetail>();

    public virtual ICollection<ProcessingModelDetail> ProcessingModelDetails { get; set; } = new List<ProcessingModelDetail>();

    public virtual ICollection<ReceivingDetail> ReceivingDetails { get; set; } = new List<ReceivingDetail>();

    public virtual ICollection<StoreTransaction> StoreTransactions { get; set; } = new List<StoreTransaction>();
}
