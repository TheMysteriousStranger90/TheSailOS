using System;
using System.Collections.Generic;

namespace TheSailOS.Users;

public class AuthenticationManager
{
    private Dictionary<string, User> users;

    public AuthenticationManager()
    {
        users = new Dictionary<string, User>();
    }

    public User CreateUser(string username, string password)
    {
        if (users.ContainsKey(username))
        {
            throw new Exception($"User {username} already exists");
        }

        var user = new User(username, password);
        users.Add(username, user);
        return user;
    }

    public User Authenticate(string username, string password)
    {
        if (users.TryGetValue(username, out var user))
        {
            if (user.VerifyPassword(password))
            {
                return user;
            }
        }

        return null;
    }
}