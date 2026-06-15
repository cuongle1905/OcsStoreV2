using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ProcessingLotInput
{
    public int Id { get; set; }

    public int Input { get; set; }

    public string Lot { get; set; }

    public sbyte Year { get; set; }

    public decimal Quantity { get; set; }

    public string Note { get; set; }

    public virtual ProcessingInput InputNavigation { get; set; }
}
