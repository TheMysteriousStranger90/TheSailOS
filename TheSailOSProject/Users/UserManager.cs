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
        private const string HomeDirectoryBase = @"0:\Home";
        private static List<User> _users = new List<User>();
        private static bool _isInitialized = false;
        public const int DefaultPasswordExpirationDays = 90;

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

                if (!Directory.Exists(HomeDirectoryBase))
                {
                    Console.WriteLine($"[INFO] Creating home directory base: {HomeDirectoryBase}");
                    try
                    {
                        Directory.CreateDirectory(HomeDirectoryBase);
                        Console.WriteLine("[INFO] Home directory base created successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Failed to create home directory base: {ex.Message}");
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

                User admin = new User("admin", adminPasswordHash, UserType.Administrator)
                {
                    PasswordExpired = true,
                    PasswordLastChanged = System.DateTime.MinValue
                };

                _users.Add(admin);
                CreateUserHomeDirectory(admin.Username);

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
            
            string validationError;
            if (!IsValidUsername(username, out validationError))
            {
                ConsoleManager.WriteLineColored($"Invalid username: {validationError}", ConsoleStyle.Colors.Error);
                return false;
            }

            if (GetUser(username) != null)
            {
                ConsoleManager.WriteLineColored($"User {username} already exists.", ConsoleStyle.Colors.Error);
                return false;
            }

            try
            {
                string passwordHash = SHA256.Hash(password);

                User newUser = new User(username, passwordHash, type)
                {
                    PasswordExpired = false,
                    PasswordLastChanged = System.DateTime.Now
                };

                _users.Add(newUser);

                CreateUserHomeDirectory(username);

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
                if (user.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
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

                if (user.PasswordExpired || IsPasswordExpired(user))
                {
                    user.PasswordExpired = true;
                    SaveUsers();

                    return true;
                }

                return true;
            }

            loggedInUser = null;
            return false;
        }

        public static bool ChangePassword(User user, string newPassword)
        {
            if (user == null)
                return false;

            try
            {
                string passwordHash = SHA256.Hash(newPassword);
                user.PasswordHash = passwordHash;
                user.PasswordExpired = false;
                user.PasswordLastChanged = System.DateTime.Now;

                SaveUsers();
                return true;
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Failed to change password: {ex.Message}", ConsoleStyle.Colors.Error);
                return false;
            }
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
                        if (parts.Length >= 3)
                        {
                            string username = parts[0];
                            string passwordHash = parts[1];
                            string type = parts[2];

                            User user = new User(username, passwordHash, type);

                            if (parts.Length >= 4)
                            {
                                bool.TryParse(parts[3], out bool passwordExpired);
                                user.PasswordExpired = passwordExpired;
                            }

                            if (parts.Length >= 5)
                            {
                                if (long.TryParse(parts[4], out long ticks) && ticks > 0)
                                {
                                    user.PasswordLastChanged = new System.DateTime(ticks);
                                }
                                else
                                {
                                    user.PasswordLastChanged = System.DateTime.MinValue;
                                }
                            }
                            else
                            {
                                user.PasswordLastChanged = System.DateTime.MinValue;
                            }

                            _users.Add(user);

                            CreateUserHomeDirectory(username);
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
                    lines.Add(
                        $"{user.Username}:{user.PasswordHash}:{user.Type}:{user.PasswordExpired}:{user.PasswordLastChanged.Ticks}");
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

        public static bool IsValidUsername(string username, out string error)
        {
            if (string.IsNullOrEmpty(username))
            {
                error = "Username cannot be empty";
                return false;
            }

            if (username.Length < 3)
            {
                error = "Username must be at least 3 characters long";
                return false;
            }

            if (username.Length > 20)
            {
                error = "Username must be at most 20 characters long";
                return false;
            }

            if (username.Contains(" "))
            {
                error = "Username cannot contain spaces";
                return false;
            }

            if (username.Contains(":"))
            {
                error = "Username cannot contain colons";
                return false;
            }

            if (username.Contains("."))
            {
                error = "Username cannot contain dots";
                return false;
            }

            if (username.Contains("~"))
            {
                error = "Username cannot contain tildes";
                return false;
            }

            if (username.Contains("/") || username.Contains("\\"))
            {
                error = "Username cannot contain path separators";
                return false;
            }

            string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";
            foreach (char c in username)
            {
                if (!allowedChars.Contains(c))
                {
                    error = $"Username contains invalid character: {c}";
                    return false;
                }
            }

            error = null;
            return true;
        }

        private static void CreateUserHomeDirectory(string username)
        {
            try
            {
                string homeDir = Path.Combine(HomeDirectoryBase, username);

                if (!Directory.Exists(homeDir))
                {
                    Directory.CreateDirectory(homeDir);
                    Console.WriteLine($"[INFO] Created home directory for user {username}: {homeDir}");

                    Directory.CreateDirectory(Path.Combine(homeDir, "Documents"));
                    Directory.CreateDirectory(Path.Combine(homeDir, "Downloads"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to create home directory for user {username}: {ex.Message}");
            }
        }

        private static bool IsPasswordExpired(User user)
        {
            if (user.PasswordLastChanged == System.DateTime.MinValue)
                return true;

            TimeSpan timeSinceLastChange = System.DateTime.Now - user.PasswordLastChanged;

            return timeSinceLastChange.TotalDays > DefaultPasswordExpirationDays;
        }

        public static bool ExpireUserPassword(string username)
        {
            User user = GetUser(username);
            if (user == null)
                return false;

            user.PasswordExpired = true;
            SaveUsers();
            return true;
        }

        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }

        public static bool SaveChanges()
        {
            try
            {
                SaveUsers();
                return true;
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Failed to save user changes: {ex.Message}",
                    ConsoleStyle.Colors.Error);
                return false;
            }
        }


        public static bool ChangeUsername(string oldUsername, string newUsername)
        {
            try
            {
                User user = GetUser(oldUsername);
                if (user == null)
                    return false;
                
                string validationError;
                if (!IsValidUsername(newUsername, out validationError))
                    return false;

                if (GetUser(newUsername) != null)
                    return false;
                
                User updatedUser = new User(newUsername, user.PasswordHash, user.Type)
                {
                    PasswordExpired = user.PasswordExpired,
                    PasswordLastChanged = user.PasswordLastChanged
                };
                
                _users.Add(updatedUser);
                
                MoveUserHomeDirectory(oldUsername, newUsername);
                
                _users.Remove(user);
                
                SaveUsers();
                
                if (Kernel.CurrentUser != null && Kernel.CurrentUser.Username == oldUsername)
                {
                    Kernel.CurrentUser = GetUser(newUsername);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to change username: {ex.Message}");
                return false;
            }
        }
        
        private static void MoveUserHomeDirectory(string oldUsername, string newUsername)
        {
            try
            {
                string oldHomeDir = Path.Combine(HomeDirectoryBase, oldUsername);
                string newHomeDir = Path.Combine(HomeDirectoryBase, newUsername);
                
                if (Directory.Exists(oldHomeDir))
                {
                    if (!Directory.Exists(newHomeDir))
                    {
                        Directory.CreateDirectory(newHomeDir);
                    }

                    // Copy all files from old to new directory
                    foreach (string file in Directory.GetFiles(oldHomeDir))
                    {
                        string fileName = Path.GetFileName(file);
                        File.Copy(file, Path.Combine(newHomeDir, fileName), true);
                    }

                    // Process all subdirectories
                    foreach (string dir in Directory.GetDirectories(oldHomeDir))
                    {
                        string dirName = new DirectoryInfo(dir).Name;
                        string newSubDir = Path.Combine(newHomeDir, dirName);

                        // Create subdirectory in destination
                        if (!Directory.Exists(newSubDir))
                        {
                            Directory.CreateDirectory(newSubDir);
                        }

                        // Copy files in subdirectory
                        foreach (string file in Directory.GetFiles(dir))
                        {
                            string fileName = Path.GetFileName(file);
                            File.Copy(file, Path.Combine(newSubDir, fileName), true);
                        }

                        // Note: This is a simple implementation that only copies one level of subdirectories
                        // A full recursive copy would be more robust for complex directory structures
                    }
                    
                    try
                    {
                        Directory.Delete(oldHomeDir, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WARNING] Failed to delete old home directory: {ex.Message}");
                    }
                }
                else
                {
                    CreateUserHomeDirectory(newUsername);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to move home directory: {ex.Message}");
            }
        }
    }
}