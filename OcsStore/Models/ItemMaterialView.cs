using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ItemMaterialView
{
    public int Item { get; set; }

    public int Material { get; set; }

    public decimal Quantity { get; set; }

    public decimal LostPercent { get; set; }

    public string Name { get; set; }

    public short Unit { get; set; }

    public sbyte MaterialGroup { get; set; }

    public bool UseLot { get; set; }

    public sbyte ItemType { get; set; }

    public string UnitName { get; set; }

    public string Lot { get; set; }

    public sbyte? Year { get; set; }

    public string LotOrdinal { get; set; }

    public decimal? Soh { get; set; }

    public string ItemName { get; set; }

    public sbyte ItemForm { get; set; }

    public sbyte ItemGroup { get; set; }
}
