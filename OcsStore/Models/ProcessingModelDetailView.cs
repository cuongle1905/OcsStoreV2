using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ProcessingModelDetailView
{
    public short Model { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public short Store { get; set; }

    public bool IsOutput { get; set; }

    public decimal Quantity { get; set; }

    public decimal LostPercent { get; set; }

    public string ItemName { get; set; }

    public bool ItemIsInput { get; set; }

    public bool ItemIsOutput { get; set; }

    public bool UseLot { get; set; }

    public string UnitName { get; set; }

    public string StoreName { get; set; }

    public string InOut { get; set; }

    public decimal Soh { get; set; }
}
