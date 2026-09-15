using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class UserGroupPermission
{
    public sbyte UserGroup { get; set; }

    public short Permission { get; set; }
}
