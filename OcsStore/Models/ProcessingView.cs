using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ProcessingView
{
    public int Id { get; set; }

    public string Lot { get; set; }

    public sbyte Year { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public short Store { get; set; }

    public decimal Quantity { get; set; }

    public string ItemName { get; set; }

    public bool UseLot { get; set; }

    public sbyte ItemGroup { get; set; }

    public bool ItemIsInput { get; set; }

    public bool ItemIsOutput { get; set; }

    public string ItemGroupName { get; set; }

    public string ProcessingName { get; set; }

    public string UnitName { get; set; }

    public string Note { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }

    public string StoreName { get; set; }

    public decimal? Soh { get; set; }
}
