using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class StockView
{
    public short Store { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public sbyte Year { get; set; }

    public string Lot { get; set; }

    public int LastTransaction { get; set; }

    public decimal? Soh { get; set; }

    public decimal? Value { get; set; }

    public decimal? Ave { get; set; }

    public long Ordinal { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }

    public string ItemName { get; set; }

    public string UnitName { get; set; }

    public sbyte ItemGroup { get; set; }

    public string GroupName { get; set; }

    public bool ItemIsInput { get; set; }

    public bool ItemIsOutput { get; set; }

    public string LotValue { get; set; }
}
