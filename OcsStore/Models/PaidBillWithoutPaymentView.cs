using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class PaidBillWithoutPaymentView
{
    public int Id { get; set; }

    public short Customer { get; set; }

    public decimal TotalValue { get; set; }

    public bool? Paid { get; set; }

    public DateTime? DatePaid { get; set; }

    public string TimePaid { get; set; }

    public short? UserPaid { get; set; }
}
