using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class CustomerDebtView
{
    public short Customer { get; set; }

    public decimal? Debt { get; set; }
}
