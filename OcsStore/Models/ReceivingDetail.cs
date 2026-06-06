using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ReceivingDetail
{
    public int Id { get; set; }

    public int Receiving { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public int Ordinal { get; set; }

    public string Note { get; set; }

    public virtual Item ItemNavigation { get; set; }

    public virtual Receiving ReceivingNavigation { get; set; }

    public virtual Unit UnitNavigation { get; set; }
}
