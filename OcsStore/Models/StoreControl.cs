using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class StoreControl
{
    public short Store { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public string Lot { get; set; }

    public decimal Quantity { get; set; }

    public DateTime Date { get; set; }
}
