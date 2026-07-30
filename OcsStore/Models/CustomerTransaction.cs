using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class CustomerTransaction
{
    public int Id { get; set; }

    public sbyte Type { get; set; }

    public int MainId { get; set; }

    public short Customer { get; set; }

    public decimal Amount { get; set; }

    public decimal Debt { get; set; }

    public DateOnly Date { get; set; }

    public string Time { get; set; }

    public short User { get; set; }

    public long Ordinal { get; set; }

    public bool IsCompleted { get; set; }
}
