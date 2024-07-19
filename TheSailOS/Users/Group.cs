using System.Collections.Generic;

namespace TheSailOS.Users;

public class Group
{
    public string Name { get; set; }
    public List<string> Permissions { get; set; }

    public Group(string name)
    {
        Name = name;
        Permissions = new List<string>();
    }
}