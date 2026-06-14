using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ItemManagementView
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

    public decimal MinSoh { get; set; }

    public string ProcessingDesc { get; set; }

    public bool AllowSale { get; set; }

    public decimal? SalePrice { get; set; }

    public bool Inactive { get; set; }

    public DateTime? DateCreated { get; set; }

    public string TimeCreated { get; set; }

    public short? UserCreated { get; set; }

    public bool? Used { get; set; }
}
