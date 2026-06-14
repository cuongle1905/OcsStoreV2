using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class UnitManagementView
{
    public short Id { get; set; }

    public string Name { get; set; }

    public string FullName { get; set; }

    public short BaseUnit { get; set; }

    public double BuExchange { get; set; }

    public bool Inactive { get; set; }

    public string Note { get; set; }

    public bool? Used { get; set; }
}
