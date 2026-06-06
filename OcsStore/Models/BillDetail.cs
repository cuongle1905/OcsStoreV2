using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class BillDetail
{
    public int Id { get; set; }

    public int Bill { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public string Note { get; set; }
}
