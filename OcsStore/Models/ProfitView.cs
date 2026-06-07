using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ProfitView
{
    public decimal BillPrice { get; set; }

    public decimal? Ave { get; set; }

    public decimal? Profit { get; set; }
}
