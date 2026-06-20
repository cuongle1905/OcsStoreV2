using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class Processing
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }

    public short Store { get; set; }

    public string Lot { get; set; }

    public sbyte Year { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public decimal Quantity { get; set; }

    public string Note { get; set; }

    public short User { get; set; }

    public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();

    public virtual Item ItemNavigation { get; set; }

    public virtual ICollection<ProcessingInput> ProcessingInputs { get; set; } = new List<ProcessingInput>();

    public virtual Store StoreNavigation { get; set; }

    public virtual Unit UnitNavigation { get; set; }

    public virtual User UserNavigation { get; set; }
}
