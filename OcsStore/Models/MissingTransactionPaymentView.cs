using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class MissingTransactionPaymentView
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public string Time { get; set; }

    public short Customer { get; set; }

    public decimal Amount { get; set; }

    public bool? IsCompleted { get; set; }

    public short? UserCreated { get; set; }
}
