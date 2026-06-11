using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class RootStockView
{
    public short? Store { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public sbyte? Year { get; set; }

    public string Lot { get; set; }

    public int? LastTransaction { get; set; }

    public decimal? Soh { get; set; }

    public decimal? Ave { get; set; }

    public decimal? Value { get; set; }

    public DateTime? Date { get; set; }

    public string Time { get; set; }

    public string ItemName { get; set; }

    public sbyte ItemGroup { get; set; }

    public sbyte AllowSale { get; set; }

    public sbyte UseLot { get; set; }

    public string LotOrdinal { get; set; }

    public int ItemOrdinal { get; set; }

    public sbyte? SohWarning { get; set; }
}
