using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class SaleItemView
{
    public bool? Selected { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public decimal? Soh { get; set; }

    public decimal? Ave { get; set; }

    public decimal? Value { get; set; }

    public string ItemName { get; set; }

    public string UnitName { get; set; }

    public decimal? SalePrice { get; set; }

    public sbyte ItemGroup { get; set; }

    public decimal? LastProcessingQuantity { get; set; }

    public DateTime? LastProcessingDate { get; set; }

    public string LastProcessingTime { get; set; }
}
