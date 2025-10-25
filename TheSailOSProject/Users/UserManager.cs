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
        private static readonly object _lockObject = new object();
        public const int DefaultPasswordExpirationDays = 90;

        public static void Initialize()
        {
            lock (_lockObject)
            {
                if (_isInitialized)
                {
                    Console.WriteLine("[INFO] UserManager already initialized");
                    return;
                }

                Console.WriteLine("[INFO] Initializing UserManager...");

                try
                {
                    EnsureDirectoryStructure();
                    
                    if (!LoadUsersFromFile())
                    {
                        Console.WriteLine("[INFO] Creating default admin user");
                        CreateDefaultAdminUser(true);
                    }

                    _isInitialized = true;
                    Console.WriteLine($"[INFO] UserManager initialized with {_users.Count} users");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] UserManager initialization failed: {ex.Message}");
                    _users = new List<User>();
                    CreateDefaultAdminUser(false);
                    _isInitialized = true;
                }
            }
        }

        private static void EnsureDirectoryStructure()
        {
            try
            {
                if (!Directory.Exists(UsersDirectory))
                {
                    Directory.CreateDirectory(UsersDirectory);
                    Console.WriteLine($"[INFO] Created users directory: {UsersDirectory}");
                }

                if (!Directory.Exists(HomeDirectoryBase))
                {
                    Directory.CreateDirectory(HomeDirectoryBase);
                    Console.WriteLine($"[INFO] Created home directory base: {HomeDirectoryBase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Directory creation issue: {ex.Message}");
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

                Console.WriteLine("[INFO] Default admin user created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to create default admin user: {ex.Message}");
            }
        }

        public static bool CreateUser(string username, string password, string type)
        {
            lock (_lockObject)
            {
                EnsureInitialized();

                if (!IsValidUsername(username, out string validationError))
                {
                    ConsoleManager.WriteLineColored($"Invalid username: {validationError}", ConsoleStyle.Colors.Error);
                    return false;
                }

                if (GetUserUnsafe(username) != null)
                {
                    ConsoleManager.WriteLineColored($"User '{username}' already exists.", ConsoleStyle.Colors.Error);
                    return false;
                }

                if (!IsValidPassword(password, out string passwordError))
                {
                    ConsoleManager.WriteLineColored($"Invalid password: {passwordError}", ConsoleStyle.Colors.Error);
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

                    Console.WriteLine($"[INFO] User '{username}' created successfully");
                    return true;
                }
                catch (Exception ex)
                {
                    ConsoleManager.WriteLineColored($"Failed to create user: {ex.Message}", ConsoleStyle.Colors.Error);
                    return false;
                }
            }
        }

        public static User GetUser(string username)
        {
            lock (_lockObject)
            {
                EnsureInitialized();
                return GetUserUnsafe(username);
            }
        }

        private static User GetUserUnsafe(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

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
            lock (_lockObject)
            {
                EnsureInitialized();

                loggedInUser = null;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("[WARNING] Login attempt with empty credentials");
                    return false;
                }

                User user = GetUserUnsafe(username);
                if (user == null)
                {
                    Console.WriteLine($"[WARNING] Login attempt for non-existent user: {username}");
                    return false;
                }

                if (!user.VerifyPassword(password))
                {
                    Console.WriteLine($"[WARNING] Failed login attempt for user: {username}");
                    return false;
                }

                loggedInUser = user;

                if (user.PasswordExpired || IsPasswordExpired(user))
                {
                    user.PasswordExpired = true;
                    SaveUsers();
                }

                Console.WriteLine($"[INFO] Successful login for user: {username}");
                return true;
            }
        }

        public static bool ChangePassword(User user, string newPassword)
        {
            lock (_lockObject)
            {
                if (user == null)
                {
                    Console.WriteLine("[ERROR] Cannot change password for null user");
                    return false;
                }

                if (!IsValidPassword(newPassword, out string error))
                {
                    ConsoleManager.WriteLineColored($"Invalid password: {error}", ConsoleStyle.Colors.Error);
                    return false;
                }

                try
                {
                    string passwordHash = SHA256.Hash(newPassword);
                    user.PasswordHash = passwordHash;
                    user.PasswordExpired = false;
                    user.PasswordLastChanged = System.DateTime.Now;

                    SaveUsers();
                    Console.WriteLine($"[INFO] Password changed for user: {user.Username}");
                    return true;
                }
                catch (Exception ex)
                {
                    ConsoleManager.WriteLineColored($"Failed to change password: {ex.Message}", ConsoleStyle.Colors.Error);
                    return false;
                }
            }
        }

        public static bool DeleteUser(string username)
        {
            lock (_lockObject)
            {
                EnsureInitialized();

                if (string.IsNullOrEmpty(username))
                    return false;

                if (username.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    ConsoleManager.WriteLineColored("Cannot delete the admin user.", ConsoleStyle.Colors.Error);
                    return false;
                }

                User userToDelete = GetUserUnsafe(username);
                if (userToDelete == null)
                {
                    ConsoleManager.WriteLineColored($"User '{username}' not found.", ConsoleStyle.Colors.Error);
                    return false;
                }

                try
                {
                    _users.Remove(userToDelete);
                    SaveUsers();
                    DeleteUserHomeDirectory(username);
                    
                    Console.WriteLine($"[INFO] User '{username}' deleted successfully");
                    return true;
                }
                catch (Exception ex)
                {
                    ConsoleManager.WriteLineColored($"Failed to delete user: {ex.Message}", ConsoleStyle.Colors.Error);
                    return false;
                }
            }
        }

        public static List<User> GetAllUsers()
        {
            lock (_lockObject)
            {
                EnsureInitialized();
                return new List<User>(_users);
            }
        }

        private static bool LoadUsersFromFile()
        {
            _users.Clear();

            try
            {
                if (!File.Exists(UsersFile))
                {
                    Console.WriteLine("[INFO] Users file not found");
                    return false;
                }

                string[] lines = File.ReadAllLines(UsersFile);
                int loadedCount = 0;

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (ParseUserLine(line, out User user))
                    {
                        _users.Add(user);
                        CreateUserHomeDirectory(user.Username);
                        loadedCount++;
                    }
                    else
                    {
                        Console.WriteLine($"[WARNING] Skipped invalid user data: {line}");
                    }
                }

                Console.WriteLine($"[INFO] Loaded {loadedCount} users from file");
                return loadedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error loading users: {ex.Message}");
                return false;
            }
        }

        private static bool ParseUserLine(string line, out User user)
        {
            user = null;

            try
            {
                string[] parts = line.Split(':');
                if (parts.Length < 3)
                    return false;

                string username = parts[0];
                string passwordHash = parts[1];
                string type = parts[2];

                user = new User(username, passwordHash, type);

                if (parts.Length >= 4)
                {
                    if (bool.TryParse(parts[3], out bool passwordExpired))
                    {
                        user.PasswordExpired = passwordExpired;
                    }
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

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void SaveUsers()
        {
            try
            {
                List<string> lines = new List<string>();
                foreach (var user in _users)
                {
                    string line = $"{user.Username}:{user.PasswordHash}:{user.Type}:{user.PasswordExpired}:{user.PasswordLastChanged.Ticks}";
                    lines.Add(line);
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
            error = null;

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

            char[] invalidChars = { ' ', ':', '.', '~', '/', '\\', '*', '?', '"', '<', '>', '|' };
            foreach (char invalidChar in invalidChars)
            {
                if (username.Contains(invalidChar))
                {
                    error = $"Username cannot contain '{invalidChar}'";
                    return false;
                }
            }

            foreach (char c in username)
            {
                bool isValid = (c >= 'a' && c <= 'z') || 
                              (c >= 'A' && c <= 'Z') || 
                              (c >= '0' && c <= '9') || 
                              c == '_' || c == '-';

                if (!isValid)
                {
                    error = $"Username contains invalid character: '{c}'";
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidPassword(string password, out string error)
        {
            error = null;

            if (string.IsNullOrEmpty(password))
            {
                error = "Password cannot be empty";
                return false;
            }

            if (password.Length < 4)
            {
                error = "Password must be at least 4 characters long";
                return false;
            }

            if (password.Length > 64)
            {
                error = "Password must be at most 64 characters long";
                return false;
            }

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
                    Directory.CreateDirectory(Path.Combine(homeDir, "Documents"));
                    Directory.CreateDirectory(Path.Combine(homeDir, "Downloads"));
                    Console.WriteLine($"[INFO] Created home directory for user: {username}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to create home directory for {username}: {ex.Message}");
            }
        }

        private static void DeleteUserHomeDirectory(string username)
        {
            try
            {
                string homeDir = Path.Combine(HomeDirectoryBase, username);

                if (Directory.Exists(homeDir))
                {
                    Directory.Delete(homeDir, true);
                    Console.WriteLine($"[INFO] Deleted home directory for user: {username}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Failed to delete home directory for {username}: {ex.Message}");
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
            lock (_lockObject)
            {
                User user = GetUserUnsafe(username);
                if (user == null)
                    return false;

                user.PasswordExpired = true;
                SaveUsers();
                Console.WriteLine($"[INFO] Password expired for user: {username}");
                return true;
            }
        }

        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }

        public static bool ChangeUsername(string oldUsername, string newUsername)
        {
            lock (_lockObject)
            {
                try
                {
                    if (string.IsNullOrEmpty(oldUsername) || string.IsNullOrEmpty(newUsername))
                        return false;

                    if (oldUsername.Equals("admin", StringComparison.OrdinalIgnoreCase))
                    {
                        ConsoleManager.WriteLineColored("Cannot change admin username.", ConsoleStyle.Colors.Error);
                        return false;
                    }

                    User user = GetUserUnsafe(oldUsername);
                    if (user == null)
                    {
                        ConsoleManager.WriteLineColored($"User '{oldUsername}' not found.", ConsoleStyle.Colors.Error);
                        return false;
                    }

                    if (!IsValidUsername(newUsername, out string validationError))
                    {
                        ConsoleManager.WriteLineColored($"Invalid new username: {validationError}", ConsoleStyle.Colors.Error);
                        return false;
                    }

                    if (GetUserUnsafe(newUsername) != null)
                    {
                        ConsoleManager.WriteLineColored($"Username '{newUsername}' already exists.", ConsoleStyle.Colors.Error);
                        return false;
                    }

                    User updatedUser = new User(newUsername, user.PasswordHash, user.Type)
                    {
                        PasswordExpired = user.PasswordExpired,
                        PasswordLastChanged = user.PasswordLastChanged
                    };

                    _users.Remove(user);
                    _users.Add(updatedUser);

                    MoveUserHomeDirectory(oldUsername, newUsername);
                    SaveUsers();

                    if (Kernel.CurrentUser != null && Kernel.CurrentUser.Username == oldUsername)
                    {
                        Kernel.CurrentUser = updatedUser;
                    }

                    Console.WriteLine($"[INFO] Username changed from '{oldUsername}' to '{newUsername}'");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to change username: {ex.Message}");
                    return false;
                }
            }
        }

        private static void MoveUserHomeDirectory(string oldUsername, string newUsername)
        {
            try
            {
                string oldHomeDir = Path.Combine(HomeDirectoryBase, oldUsername);
                string newHomeDir = Path.Combine(HomeDirectoryBase, newUsername);

                if (!Directory.Exists(oldHomeDir))
                {
                    CreateUserHomeDirectory(newUsername);
                    return;
                }

                if (Directory.Exists(newHomeDir))
                {
                    Console.WriteLine($"[WARNING] Destination directory already exists: {newHomeDir}");
                }
                else
                {
                    Directory.CreateDirectory(newHomeDir);
                }

                CopyDirectoryContents(oldHomeDir, newHomeDir);

                try
                {
                    Directory.Delete(oldHomeDir, true);
                    Console.WriteLine($"[INFO] Moved home directory from '{oldUsername}' to '{newUsername}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Could not remove old directory: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to move home directory: {ex.Message}");
                CreateUserHomeDirectory(newUsername);
            }
        }

        private static void CopyDirectoryContents(string sourceDir, string destDir)
        {
            try
            {
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                foreach (string file in Directory.GetFiles(sourceDir))
                {
                    string filename = Path.GetFileName(file);
                    string destFile = Path.Combine(destDir, filename);
                    File.Copy(file, destFile, true);
                }

                foreach (string subDir in Directory.GetDirectories(sourceDir))
                {
                    string dirName = Path.GetFileName(subDir);
                    string destSubDir = Path.Combine(destDir, dirName);
                    CopyDirectoryContents(subDir, destSubDir);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Error copying directory contents: {ex.Message}");
            }
        }

        public static int GetUserCount()
        {
            lock (_lockObject)
            {
                EnsureInitialized();
                return _users.Count;
            }
        }

        public static bool UserExists(string username)
        {
            lock (_lockObject)
            {
                EnsureInitialized();
                return GetUserUnsafe(username) != null;
            }
        }

        public static bool SaveChanges()
        {
            lock (_lockObject)
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
        }

        public static bool UpdateUserType(string username, string newType)
        {
            lock (_lockObject)
            {
                EnsureInitialized();

                User user = GetUserUnsafe(username);
                if (user == null)
                {
                    ConsoleManager.WriteLineColored($"User '{username}' not found.", ConsoleStyle.Colors.Error);
                    return false;
                }

                if (username.Equals("admin", StringComparison.OrdinalIgnoreCase) && 
                    newType != UserType.Administrator)
                {
                    ConsoleManager.WriteLineColored("Cannot change admin user type.", ConsoleStyle.Colors.Error);
                    return false;
                }

                if (newType != UserType.Administrator && newType != UserType.Standard)
                {
                    ConsoleManager.WriteLineColored($"Invalid user type: {newType}", ConsoleStyle.Colors.Error);
                    return false;
                }

                try
                {
                    user.Type = newType;
                    SaveUsers();
                    Console.WriteLine($"[INFO] User type changed for '{username}' to '{newType}'");
                    return true;
                }
                catch (Exception ex)
                {
                    ConsoleManager.WriteLineColored($"Failed to update user type: {ex.Message}", 
                        ConsoleStyle.Colors.Error);
                    return false;
                }
            }
        }
    }
}