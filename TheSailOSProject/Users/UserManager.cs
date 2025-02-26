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
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            Console.WriteLine("[INFO] Initializing UserManager...");
            
            try
            {
                if (!Directory.Exists(UsersDirectory))
                {
                    Console.WriteLine($"[INFO] Creating users directory: {UsersDirectory}");
                    try
                    {
                        Directory.CreateDirectory(UsersDirectory);
                        Console.WriteLine("[INFO] Users directory created successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Failed to create users directory: {ex.Message}");
                        Console.WriteLine($"[ERROR] Exception type: {ex.GetType().Name}");

                        _users = new List<User>();
                        CreateDefaultAdminUser(false);
                        _isInitialized = true;
                        return;
                    }
                }
                
                Console.WriteLine($"[INFO] Checking for users file: {UsersFile}");
                bool fileExists = false;
                try
                {
                    fileExists = File.Exists(UsersFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Error checking for users file: {ex.Message}");
                }
                
                if (!fileExists)
                {
                    Console.WriteLine("[INFO] Users file not found, creating default admin user");
                    CreateDefaultAdminUser(true);
                }
                else
                {
                    Console.WriteLine("[INFO] Users file found, loading users");
                    LoadUsers();
                }
                
                _isInitialized = true;
                Console.WriteLine("[INFO] UserManager initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UserManager initialization failed: {ex.Message}");
                Console.WriteLine($"[ERROR] Exception type: {ex.GetType().Name}");
                _users = new List<User>();
                CreateDefaultAdminUser(false);
                _isInitialized = true;
            }
        }

        private static void CreateDefaultAdminUser(bool saveToFile = true)
        {
            Console.WriteLine("[INFO] Creating default admin user");
            try
            {
                string adminPasswordHash = SHA256.Hash("admin");
                User admin = new User("admin", adminPasswordHash, UserType.Administrator);
                _users.Add(admin);
                
                if (saveToFile)
                {
                    SaveUsers();
                }
                else
                {
                    Console.WriteLine("[INFO] Default admin user created in memory only");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to create default admin user: {ex.Message}");
            }
        }

        public static bool CreateUser(string username, string password, string type)
        {
            EnsureInitialized();
            
            if (GetUser(username) != null)
            {
                ConsoleManager.WriteLineColored($"User {username} already exists.", ConsoleStyle.Colors.Error);
                return false;
            }

            try
            {
                string passwordHash = SHA256.Hash(password);
                User newUser = new User(username, passwordHash, type);
                _users.Add(newUser);
                SaveUsers();
                return true;
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Failed to create user: {ex.Message}", ConsoleStyle.Colors.Error);
                return false;
            }
        }

        public static User GetUser(string username)
        {
            EnsureInitialized();
            
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
            EnsureInitialized();
            
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
            EnsureInitialized();
            
            User userToDelete = GetUser(username);
            if (userToDelete == null)
            {
                ConsoleManager.WriteLineColored($"User {username} not found.", ConsoleStyle.Colors.Error);
                return false;
            }

            try
            {
                _users.Remove(userToDelete);
                SaveUsers();
                return true;
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Failed to delete user: {ex.Message}", ConsoleStyle.Colors.Error);
                return false;
            }
        }

        public static List<User> GetAllUsers()
        {
            EnsureInitialized();
            
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
                            Console.WriteLine($"[WARNING] Invalid user data in users file: {line}");
                        }
                    }
                    
                    Console.WriteLine($"[INFO] Loaded {_users.Count} users from file");
                }
                else
                {
                    Console.WriteLine("[WARNING] Users file not found during load");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error loading users: {ex.Message}");
                Console.WriteLine("[INFO] Creating default admin user after load failure");
                
                if (!_users.Any(u => u.Username == "admin" && u.Type == UserType.Administrator))
                {
                    CreateDefaultAdminUser(false);
                }
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
                
                string directory = Path.GetDirectoryName(UsersFile);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllLines(UsersFile, lines.ToArray());
                Console.WriteLine($"[INFO] Saved {lines.Count} users to file");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error saving users: {ex.Message}");
            }
        }
        
        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }
    }
}