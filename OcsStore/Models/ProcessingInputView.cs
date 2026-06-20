using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ProcessingInputView
{
    public int Id { get; set; }

    public int Processing { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }

    public int Item { get; set; }

    public string Lot { get; set; }

    public double? ItemLotName { get; set; }

    public string ItemName { get; set; }

    public decimal Quantity { get; set; }

    public short Unit { get; set; }

    public string UnitName { get; set; }

    public sbyte ItemGroup { get; set; }

    public int Material { get; set; }

    public decimal MaterialQuantity { get; set; }

    public string MaterialName { get; set; }

    public bool MaterialUseLot { get; set; }
}
