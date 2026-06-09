using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ProcessingInputView
{
    public int Id { get; set; }

    public int Processing { get; set; }

    public string Lot { get; set; }

    public sbyte Year { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public short Store { get; set; }

    public decimal Quantity { get; set; }

    public string ItemName { get; set; }

    public bool UseLot { get; set; }

    public sbyte ItemType { get; set; }

    public string UnitName { get; set; }

    public string Note { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }

    public string StoreName { get; set; }

    public decimal? Soh { get; set; }

    public decimal? MaterialQuantity { get; set; }

    public decimal? MaterialLostPercent { get; set; }
}
