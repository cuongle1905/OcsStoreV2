using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class UserGroup
{
    public sbyte Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
