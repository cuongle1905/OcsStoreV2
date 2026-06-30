using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ItemForm
{
    public sbyte Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
