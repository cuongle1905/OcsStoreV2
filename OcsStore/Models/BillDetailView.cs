using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class BillDetailView
{
    public int Id { get; set; }

    public int Bill { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Value { get; set; }

    public string Note { get; set; }

    public string ItemName { get; set; }

    public string UnitName { get; set; }

    public decimal? Soh { get; set; }

    public decimal? Ave { get; set; }

    public short? StockUnit { get; set; }

    public string StockUnitName { get; set; }
}
