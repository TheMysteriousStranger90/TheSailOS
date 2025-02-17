using TheSailOSProject.Security;

namespace TheSailOSProject.Users;

public class User
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public UserType Type { get; set; }

    public User(string username, string password, UserType type)
    {
        Username = username;
        PasswordHash = SHA256.Hash(password);
        Type = type;
    }

    public bool VerifyPassword(string password)
    {
        return SHA256.Hash(password) == PasswordHash;
    }
}