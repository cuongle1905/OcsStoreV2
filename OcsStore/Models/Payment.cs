using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class Payment
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public string Time { get; set; }

    public short Customer { get; set; }

    public decimal Amount { get; set; }

    public bool? IsCompleted { get; set; }

    public short UserCreated { get; set; }

    public DateOnly? DateCreated { get; set; }

    public string TimeCreated { get; set; }

    public virtual Customer CustomerNavigation { get; set; }

    public virtual ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();

    public virtual User UserCreatedNavigation { get; set; }
}
