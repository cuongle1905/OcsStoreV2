using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class BillLotDetail
{
    public int Id { get; set; }

    public int BillDetail { get; set; }

    public string Lot { get; set; }

    public sbyte Year { get; set; }

    public decimal Quantity { get; set; }

    public string Note { get; set; }

    public virtual BillDetail BillDetailNavigation { get; set; }
}
