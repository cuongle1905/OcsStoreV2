using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ItemGroup
{
    public sbyte Id { get; set; }

    public string Name { get; set; }

    public bool IsInput { get; set; }

    public bool IsOutput { get; set; }

    public string ProcessingName { get; set; }

    public sbyte Ordinal { get; set; }
}
