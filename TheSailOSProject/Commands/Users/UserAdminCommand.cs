using System;
using TheSailOSProject.Logging;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Users
{
    public class UserAdminCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (Kernel.CurrentUser == null || Kernel.CurrentUser.Type != UserType.Administrator)
            {
                ConsoleManager.WriteLineColored("This command requires administrator privileges.", 
                    ConsoleStyle.Colors.Error);
                Log.WriteLog(LogPriority.Warning, "UserAdmin", 
                    "Unauthorized access attempt to useradmin command", 
                    Kernel.CurrentUser?.Username ?? "Unknown");
                return;
            }

            if (args.Length < 2)
            {
                ShowUsage();
                return;
            }

            string action = args[0].ToLower();
            string username = args[1];

            if (string.IsNullOrWhiteSpace(username))
            {
                ConsoleManager.WriteLineColored("Username cannot be empty.", ConsoleStyle.Colors.Error);
                return;
            }

            User targetUser = UserManager.GetUser(username);
            if (targetUser == null)
            {
                ConsoleManager.WriteLineColored($"User '{username}' not found.", ConsoleStyle.Colors.Error);
                return;
            }

            switch (action)
            {
                case "grant":
                    GrantAdminPrivileges(targetUser);
                    break;
                
                case "revoke":
                    RevokeAdminPrivileges(targetUser);
                    break;
                
                case "info":
                    ShowUserInfo(targetUser);
                    break;
                
                default:
                    ConsoleManager.WriteLineColored($"Unknown action: {action}", ConsoleStyle.Colors.Error);
                    ShowUsage();
                    break;
            }
        }

        private void GrantAdminPrivileges(User targetUser)
        {
            if (targetUser.Type == UserType.Administrator)
            {
                ConsoleManager.WriteLineColored($"User '{targetUser.Username}' already has administrator privileges.", 
                    ConsoleStyle.Colors.Warning);
                return;
            }

            if (UserManager.UpdateUserType(targetUser.Username, UserType.Administrator))
            {
                ConsoleManager.WriteLineColored($"Administrator privileges granted to '{targetUser.Username}'.", 
                    ConsoleStyle.Colors.Success);
                Log.WriteLog(LogPriority.Info, "UserAdmin", 
                    $"Administrator privileges granted to '{targetUser.Username}' by '{Kernel.CurrentUser.Username}'", 
                    Kernel.CurrentUser.Username);
            }
            else
            {
                ConsoleManager.WriteLineColored("Failed to grant administrator privileges.", 
                    ConsoleStyle.Colors.Error);
            }
        }

        private void RevokeAdminPrivileges(User targetUser)
        {
            if (targetUser.Username.Equals(Kernel.CurrentUser.Username, StringComparison.OrdinalIgnoreCase))
            {
                ConsoleManager.WriteLineColored("You cannot revoke your own administrator privileges.", 
                    ConsoleStyle.Colors.Error);
                return;
            }

            if (targetUser.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleManager.WriteLineColored("Cannot revoke administrator privileges from the default admin user.", 
                    ConsoleStyle.Colors.Error);
                return;
            }

            if (targetUser.Type != UserType.Administrator)
            {
                ConsoleManager.WriteLineColored($"User '{targetUser.Username}' does not have administrator privileges.", 
                    ConsoleStyle.Colors.Warning);
                return;
            }

            if (UserManager.UpdateUserType(targetUser.Username, UserType.Standard))
            {
                ConsoleManager.WriteLineColored($"Administrator privileges revoked from '{targetUser.Username}'.", 
                    ConsoleStyle.Colors.Success);
                Log.WriteLog(LogPriority.Info, "UserAdmin", 
                    $"Administrator privileges revoked from '{targetUser.Username}' by '{Kernel.CurrentUser.Username}'", 
                    Kernel.CurrentUser.Username);
            }
            else
            {
                ConsoleManager.WriteLineColored("Failed to revoke administrator privileges.", 
                    ConsoleStyle.Colors.Error);
            }
        }

        private void ShowUserInfo(User user)
        {
            ConsoleManager.WriteLineColored($"\nUser Information for '{user.Username}':", 
                ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored($"  Type: {user.Type}", ConsoleStyle.Colors.Command);
            ConsoleManager.WriteLineColored($"  Home Directory: {user.HomeDirectory}", 
                ConsoleStyle.Colors.Command);
            ConsoleManager.WriteLineColored($"  Password Expired: {user.PasswordExpired}", 
                ConsoleStyle.Colors.Command);
            
            if (user.PasswordLastChanged != System.DateTime.MinValue)
            {
                ConsoleManager.WriteLineColored($"  Password Last Changed: {user.PasswordLastChanged}", 
                    ConsoleStyle.Colors.Command);
                
                TimeSpan passwordAge = System.DateTime.Now - user.PasswordLastChanged;
                ConsoleManager.WriteLineColored($"  Password Age: {(int)passwordAge.TotalDays} days", 
                    ConsoleStyle.Colors.Command);
            }
            else
            {
                ConsoleManager.WriteLineColored("  Password Last Changed: Never", 
                    ConsoleStyle.Colors.Warning);
            }
            
            Console.WriteLine();
        }

        private void ShowUsage()
        {
            ConsoleManager.WriteLineColored("\nUser Administration Command", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored("Usage:", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored("  useradmin grant <username>  - Grant administrator privileges", 
                ConsoleStyle.Colors.Command);
            ConsoleManager.WriteLineColored("  useradmin revoke <username> - Revoke administrator privileges", 
                ConsoleStyle.Colors.Command);
            ConsoleManager.WriteLineColored("  useradmin info <username>   - Display user information", 
                ConsoleStyle.Colors.Command);
            Console.WriteLine();
        }
    }
}