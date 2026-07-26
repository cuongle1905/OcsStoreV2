using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class PaymentDetail
{
    public int Payment { get; set; }

    public int Bill { get; set; }

    public decimal Amount { get; set; }

    public bool PaidFullBill { get; set; }

    public virtual Bill BillNavigation { get; set; }

    public virtual Payment PaymentNavigation { get; set; }
}
