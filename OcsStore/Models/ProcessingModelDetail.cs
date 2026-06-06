using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ProcessingModelDetail
{
    public short Model { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public short Store { get; set; }

    public bool IsOutput { get; set; }

    public decimal Quantity { get; set; }

    public decimal LostPercent { get; set; }

    public virtual Item ItemNavigation { get; set; }

    public virtual Unit UnitNavigation { get; set; }
}
