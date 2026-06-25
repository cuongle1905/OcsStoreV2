using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ItemLastProcessingView
{
    public int Item { get; set; }

    public int? LastProcessing { get; set; }

    public decimal Quantity { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }
}
