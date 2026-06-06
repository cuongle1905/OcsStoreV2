using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class Bill
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }

    public short Customer { get; set; }

    public string CustomerName { get; set; }

    public string CustomerAddress { get; set; }

    public string CustomerPhone { get; set; }

    public string CustomerEmail { get; set; }

    public string Note { get; set; }

    public decimal TotalValue { get; set; }

    public DateTime? DatePaid { get; set; }

    public string TimePaid { get; set; }

    public short? UserPaid { get; set; }

    public DateTime DateCreated { get; set; }

    public string TimeCreated { get; set; }

    public short UserCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public string TimeModified { get; set; }

    public short? UserModified { get; set; }

    public virtual Customer CustomerNavigation { get; set; }

    public virtual User UserModifiedNavigation { get; set; }
}
