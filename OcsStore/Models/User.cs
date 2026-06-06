using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class User
{
    public short Id { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public bool IsSuper { get; set; }

    public string Token { get; set; }

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<Processing> Processings { get; set; } = new List<Processing>();

    public virtual ICollection<Receiving> Receivings { get; set; } = new List<Receiving>();

    public virtual ICollection<StoreTransaction> StoreTransactions { get; set; } = new List<StoreTransaction>();
}
