using System;
using TheSailOSProject.Security;

namespace TheSailOSProject.Users
{
    public class User
    {
        public string Username { get; private set; }
        public string PasswordHash { get; set; }
        public string Type { get; set; }
        public bool PasswordExpired { get; set; }
        public System.DateTime PasswordLastChanged { get; set; }
        public string HomeDirectory => $"0:\\Home\\{Username}";

        public User(string username, string passwordHash, string type)
        {
            Username = username;
            PasswordHash = passwordHash;

            if (type == UserType.Administrator || type == UserType.Standard)
            {
                Type = type;
            }
            else
            {
                Type = UserType.Standard;
            }

            PasswordExpired = false;
            PasswordLastChanged = System.DateTime.Now;
        }

        public bool VerifyPassword(string password)
        {
            string hashedInput = SHA256.Hash(password);
            return PasswordHash.Equals(hashedInput);
        }

        public bool IsAdministrator()
        {
            return Type == UserType.Administrator;
        }

        public override string ToString()
        {
            return $"{Username} ({Type})";
        }
    }
}