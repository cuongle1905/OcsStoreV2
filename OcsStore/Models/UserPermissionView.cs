using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class UserPermissionView
{
    public short UserId { get; set; }

    public sbyte? UserGroup { get; set; }

    public short Permission { get; set; }

    public string Name { get; set; }

    public string Page { get; set; }
}
