using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class LastStoreTransaction
{
    public short Store { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public sbyte Year { get; set; }

    public string Lot { get; set; }

    public int LastTransaction { get; set; }

    public virtual Item ItemNavigation { get; set; }

    public virtual StoreTransaction LastTransactionNavigation { get; set; }

    public virtual Store StoreNavigation { get; set; }

    public virtual Unit UnitNavigation { get; set; }
}
