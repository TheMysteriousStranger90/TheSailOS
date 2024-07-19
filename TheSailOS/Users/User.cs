using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace TheSailOS.Users;

public class User
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Salt { get; private set; }
    public List<Group> Groups { get; set; }

    public User(string username, string password)
    {
        Username = username;
        Salt = GenerateSalt();
        PasswordHash = ComputeHash(password, Salt);
        Groups = new List<Group>();
    }

    private string GenerateSalt()
    {
        var bytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes);
    }

    private string ComputeHash(string input, string salt)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(input + salt);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    public bool VerifyPassword(string password)
    {
        return PasswordHash == ComputeHash(password, Salt);
    }
}