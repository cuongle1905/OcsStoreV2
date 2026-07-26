using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class MissingTransactionBillView
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }

    public short Customer { get; set; }

    public decimal TotalValue { get; set; }

    public short UserCreated { get; set; }
}
