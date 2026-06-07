using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class Store
{
    public short Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<LastStoreTransaction> LastStoreTransactions { get; set; } = new List<LastStoreTransaction>();

    public virtual ICollection<Receiving> Receivings { get; set; } = new List<Receiving>();

    public virtual ICollection<StoreTransaction> StoreTransactions { get; set; } = new List<StoreTransaction>();
}
