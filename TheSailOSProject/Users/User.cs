using TheSailOSProject.Security;

namespace TheSailOSProject.Users;

public class User
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Type { get; set; }

    public User(string username, string password, string type)
    {
        Username = username;
        PasswordHash = password;
        Type = type;
    }

    public bool VerifyPassword(string password)
    {
        return SHA256.Hash(password) == PasswordHash;
    }
}