using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class BillDetail
{
    public int Id { get; set; }

    public int Bill { get; set; }

    public int Item { get; set; }

    public string ItemName { get; set; }

    public short Unit { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Discount { get; set; }

    public string Note { get; set; }

    public int Ordinal { get; set; }

    public int? Processing { get; set; }

    public string Type { get; set; }

    public string Name { get; set; }

    public virtual ICollection<BillLotDetail> BillLotDetails { get; set; } = new List<BillLotDetail>();

    public virtual Bill BillNavigation { get; set; }

    public virtual Item ItemNavigation { get; set; }

    public virtual Processing ProcessingNavigation { get; set; }
}
