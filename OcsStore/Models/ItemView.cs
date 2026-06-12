using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ItemView
{
    public int Id { get; set; }

    public sbyte Group { get; set; }

    public string Name { get; set; }

    public string FullName { get; set; }

    public string Code { get; set; }

    public short Unit { get; set; }

    public string Note { get; set; }

    public int Ordinal { get; set; }

    public bool UseLot { get; set; }

    public string ProcessingDesc { get; set; }

    public bool Inactive { get; set; }

    public string UnitName { get; set; }

    public string GroupName { get; set; }

    public sbyte ItemType { get; set; }

    public string ProcessingName { get; set; }

    public bool AllowSale { get; set; }
}
