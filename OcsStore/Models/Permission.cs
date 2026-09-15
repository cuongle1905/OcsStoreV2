using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class Permission
{
    public short Id { get; set; }

    public string Name { get; set; }

    public string Page { get; set; }
}
