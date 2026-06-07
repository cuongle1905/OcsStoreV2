using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ItemMaterial
{
    public int Item { get; set; }

    public int Material { get; set; }

    public decimal Quantity { get; set; }

    public decimal LostPercent { get; set; }

    public virtual Item ItemNavigation { get; set; }

    public virtual Item MaterialNavigation { get; set; }
}
