using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class Processing
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }

    public short Model { get; set; }

    public short User { get; set; }

    public virtual ProcessingModel ModelNavigation { get; set; }

    public virtual ICollection<ProcessingDetail> ProcessingDetails { get; set; } = new List<ProcessingDetail>();

    public virtual User UserNavigation { get; set; }
}
