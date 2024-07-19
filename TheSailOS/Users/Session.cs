using System;

namespace TheSailOS.Users;

public class Session
{
    public string Id { get; private set; }
    public User User { get; private set; }

    public Session(User user)
    {
        Id = Guid.NewGuid().ToString();
        User = user;
    }
}