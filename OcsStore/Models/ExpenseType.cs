using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ExpenseType
{
    public sbyte Id { get; set; }

    public string Name { get; set; }

    public sbyte Ordinal { get; set; }

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
