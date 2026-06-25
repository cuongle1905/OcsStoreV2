using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class ItemLastProcessingIdView
{
    public int Item { get; set; }

    public int? LastProcessing { get; set; }
}
