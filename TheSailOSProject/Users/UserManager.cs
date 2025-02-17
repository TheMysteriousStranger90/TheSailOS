using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheSailOSProject.Security;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Users
{
    public static class UserManager
    {
        private const string UsersDirectory = @"0:\System\Users";
        private const string UsersFile = @"0:\System\Users\usersthesail.dat";
        private static List<User> _users = new List<User>();

        public static void Initialize()
        {
            if (!Directory.Exists(UsersDirectory))
            {
                Directory.CreateDirectory(UsersDirectory);
            }

            if (!File.Exists(UsersFile))
            {
                CreateDefaultAdminUser();
            }

            LoadUsers();
        }

        private static void CreateDefaultAdminUser()
        {
            string adminPasswordHash = SHA256.Hash("admin");
            User admin = new User("admin", adminPasswordHash, UserType.Administrator);
            _users.Add(admin);
            SaveUsers();
        }

        public static bool CreateUser(string username, string password, string type)
        {
            if (GetUser(username) != null)
            {
                ConsoleManager.WriteLineColored($"User {username} already exists.", ConsoleStyle.Colors.Error);
                return false;
            }

            string passwordHash = SHA256.Hash(password);
            User newUser = new User(username, passwordHash, type);
            _users.Add(newUser);
            SaveUsers();
            return true;
        }

        public static User GetUser(string username)
        {
            foreach (var user in _users)
            {
                if (user.Username.Equals(username))
                {
                    return user;
                }
            }

            return null;
        }

        public static bool VerifyLogin(string username, string password, out User loggedInUser)
        {
            User user = GetUser(username);
            if (user != null && user.VerifyPassword(password))
            {
                loggedInUser = user;
                return true;
            }

            loggedInUser = null;
            return false;
        }

        public static bool DeleteUser(string username)
        {
            User userToDelete = GetUser(username);
            if (userToDelete == null)
            {
                ConsoleManager.WriteLineColored($"User {username} not found.", ConsoleStyle.Colors.Error);
                return false;
            }

            _users.Remove(userToDelete);
            SaveUsers();
            return true;
        }

        public static List<User> GetAllUsers()
        {
            return _users.ToList();
        }

        private static void LoadUsers()
        {
            _users.Clear();

            try
            {
                if (File.Exists(UsersFile))
                {
                    string[] lines = File.ReadAllLines(UsersFile);
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split(':');
                        if (parts.Length == 3)
                        {
                            string username = parts[0];
                            string passwordHash = parts[1];
                            string type = parts[2];
                            User user = new User(username, passwordHash, type);
                            _users.Add(user);
                        }
                        else
                        {
                            ConsoleManager.WriteLineColored($"Invalid user data in users file: {line}",
                                ConsoleStyle.Colors.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error loading users: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }

        private static void SaveUsers()
        {
            try
            {
                List<string> lines = new List<string>();
                foreach (var user in _users)
                {
                    lines.Add($"{user.Username}:{user.PasswordHash}:{user.Type}");
                }

                File.WriteAllLines(UsersFile, lines.ToArray());
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error saving users: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }
    }
}