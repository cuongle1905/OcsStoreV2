using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class Item
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

    public virtual ICollection<ItemMaterial> ItemMaterialItemNavigations { get; set; } = new List<ItemMaterial>();

    public virtual ICollection<ItemMaterial> ItemMaterialMaterialNavigations { get; set; } = new List<ItemMaterial>();

    public virtual ICollection<LastStoreTransaction> LastStoreTransactions { get; set; } = new List<LastStoreTransaction>();

    public virtual ICollection<ProcessingInput> ProcessingInputs { get; set; } = new List<ProcessingInput>();

    public virtual ICollection<Processing> Processings { get; set; } = new List<Processing>();

    public virtual ICollection<ReceivingDetail> ReceivingDetails { get; set; } = new List<ReceivingDetail>();

    public virtual ICollection<StoreTransaction> StoreTransactions { get; set; } = new List<StoreTransaction>();
}
