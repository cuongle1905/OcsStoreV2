using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class CustomerManagementView
{
    public short Id { get; set; }

    public string Phone { get; set; }

    public string Name { get; set; }

    public string Address { get; set; }

    public string Email { get; set; }

    public string Note { get; set; }

    public bool? Used { get; set; }
}
