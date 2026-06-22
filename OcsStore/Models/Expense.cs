using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class Expense
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public sbyte Type { get; set; }

    public string Content { get; set; }

    public decimal Amount { get; set; }

    public string Actor { get; set; }

    public string Note { get; set; }

    public virtual ExpenseType TypeNavigation { get; set; }
}
