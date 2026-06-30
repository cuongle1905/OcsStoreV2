using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ItemCoupleMaterialView
{
    public bool? Selected { get; set; }

    public int Item { get; set; }

    public string ItemName { get; set; }

    public string CalculatedName { get; set; }

    public int Material1 { get; set; }

    public decimal Quantity1 { get; set; }

    public string MaterialName1 { get; set; }

    public int Material2 { get; set; }

    public decimal Quantity2 { get; set; }

    public string MaterialName2 { get; set; }

    public sbyte ItemForm { get; set; }
}
