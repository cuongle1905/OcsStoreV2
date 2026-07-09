using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class InventoryDetail
{
    public int Id { get; set; }

    public int Inventory { get; set; }

    public int Item { get; set; }

    public short Unit { get; set; }

    public decimal Soh { get; set; }

    public decimal Ave { get; set; }

    public virtual Inventory InventoryNavigation { get; set; }
}
