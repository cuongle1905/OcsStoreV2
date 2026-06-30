using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class BillMinDetailIdView
{
    public int Bill { get; set; }

    public int? MinId { get; set; }

    public long Count { get; set; }
}
