using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class Receiving
{
    public int Id { get; set; }

    public short Store { get; set; }

    public DateTime Date { get; set; }

    public string Time { get; set; }

    public short User { get; set; }

    public virtual ICollection<ReceivingDetail> ReceivingDetails { get; set; } = new List<ReceivingDetail>();

    public virtual Store StoreNavigation { get; set; }

    public virtual User UserNavigation { get; set; }
}
