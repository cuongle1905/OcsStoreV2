using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class Customer
{
    public short Id { get; set; }

    public string Phone { get; set; }

    public string Name { get; set; }

    public string Address { get; set; }

    public string Email { get; set; }

    public string Note { get; set; }

    public bool Inactive { get; set; }

    public decimal Debt { get; set; }

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
