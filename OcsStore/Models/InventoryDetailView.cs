using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class InventoryDetailView
{
    public bool? Selected { get; set; }

    public int Id { get; set; }

    public int Inventory { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public decimal Soh { get; set; }

    public decimal Ave { get; set; }

    public string ItemName { get; set; }

    public bool UseLot { get; set; }

    public sbyte ItemGroup { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }

    public short UserCreated { get; set; }
}
