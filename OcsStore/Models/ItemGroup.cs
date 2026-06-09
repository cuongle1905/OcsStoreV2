using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ItemGroup
{
    public sbyte Id { get; set; }

    public sbyte Type { get; set; }

    public string Name { get; set; }

    public string ProcessingName { get; set; }

    public sbyte Ordinal { get; set; }
}
