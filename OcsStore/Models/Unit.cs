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

    public bool Inactive { get; set; }

    public string Note { get; set; }

    public virtual ICollection<LastStoreTransaction> LastStoreTransactions { get; set; } = new List<LastStoreTransaction>();

    public virtual ICollection<ProcessingInput> ProcessingInputs { get; set; } = new List<ProcessingInput>();

    public virtual ICollection<Processing> Processings { get; set; } = new List<Processing>();

    public virtual ICollection<ReceivingDetail> ReceivingDetails { get; set; } = new List<ReceivingDetail>();

    public virtual ICollection<StoreTransaction> StoreTransactions { get; set; } = new List<StoreTransaction>();
}
