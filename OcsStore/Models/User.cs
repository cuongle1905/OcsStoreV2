using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class User
{
    public short Id { get; set; }

    public string Username { get; set; }

    public string Name { get; set; }

    public string Password { get; set; }

    public bool IsAdmin { get; set; }

    public string Token { get; set; }

    public sbyte? Group { get; set; }

    public bool Inactive { get; set; }

    public virtual ICollection<Bill> BillUserCreatedNavigations { get; set; } = new List<Bill>();

    public virtual ICollection<Bill> BillUserModifiedNavigations { get; set; } = new List<Bill>();

    public virtual UserGroup GroupNavigation { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Processing> Processings { get; set; } = new List<Processing>();

    public virtual ICollection<Receiving> Receivings { get; set; } = new List<Receiving>();

    public virtual ICollection<StoreTransaction> StoreTransactions { get; set; } = new List<StoreTransaction>();
}
