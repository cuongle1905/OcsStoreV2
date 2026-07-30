using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class CustomerBillingView
{
    public short Id { get; set; }

    public string Name { get; set; }

    public decimal Debt { get; set; }
}
