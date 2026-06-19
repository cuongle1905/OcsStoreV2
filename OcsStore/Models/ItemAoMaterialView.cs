using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ItemAoMaterialView
{
    public int Item { get; set; }

    public int Material { get; set; }

    public decimal Quantity { get; set; }

    public string MaterialName { get; set; }

    public string ItemName { get; set; }
}
