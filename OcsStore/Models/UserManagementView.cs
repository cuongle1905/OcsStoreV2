using System;
using System.Collections.Generic;

namespace OcsStore.Models;

public partial class UserManagementView
{
    public short Id { get; set; }

    public string Username { get; set; }

    public string Name { get; set; }

    public string Password { get; set; }

    public bool IsAdmin { get; set; }

    public string Token { get; set; }

    public sbyte? Group { get; set; }

    public bool Inactive { get; set; }
}
