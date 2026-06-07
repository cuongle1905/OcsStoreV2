using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ReceivingDetailView
{
    public int Id { get; set; }

    public int Receiving { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }

    public int Item { get; set; }

    public string ItemName { get; set; }

    public short Unit { get; set; }

    public string UnitName { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public int Ordinal { get; set; }

    public string Note { get; set; }

    public decimal Value { get; set; }
}
