using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ProcessingView
{
    public bool? Selected { get; set; }

    public int Id { get; set; }

    public string Lot { get; set; }

    public sbyte Year { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public short Store { get; set; }

    public decimal Quantity { get; set; }

    public string ItemName { get; set; }

    public sbyte ItemGroup { get; set; }

    public bool UseLot { get; set; }

    public decimal? SalePrice { get; set; }

    public string UnitName { get; set; }

    public string Note { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }

    public short User { get; set; }

    public string UserName { get; set; }

    public DateTime? DateCreated { get; set; }

    public string TimeCreated { get; set; }

    public bool? AllowDelete { get; set; }
}
