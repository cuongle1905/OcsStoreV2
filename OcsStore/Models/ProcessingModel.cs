using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ProcessingModel
{
    public short Id { get; set; }

    public sbyte Type { get; set; }

    public string Name { get; set; }

    public short Ordinal { get; set; }

    public string Description { get; set; }

    public virtual ICollection<Processing> Processings { get; set; } = new List<Processing>();
}
