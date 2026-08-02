using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class BillSummaryView
{
    public int Bill { get; set; }

    public double? TotalQtyPerBu { get; set; }

    public int? MinId { get; set; }

    public long Count { get; set; }
}
