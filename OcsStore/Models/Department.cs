using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class Department
{
    public sbyte Id { get; set; }

    public string Name { get; set; }

    public sbyte Ordinal { get; set; }
}
