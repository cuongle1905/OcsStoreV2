using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class Inventory
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }

    public short Store { get; set; }

    public short UserCreated { get; set; }

    public virtual ICollection<InventoryDetail> InventoryDetails { get; set; } = new List<InventoryDetail>();
}
