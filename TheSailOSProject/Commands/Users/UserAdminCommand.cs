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
                return;
            }

            if (args.Length < 2)
            {
                ShowUsage();
                return;
            }

            string action = args[0].ToLower();
            string username = args[1];

            User targetUser = UserManager.GetUser(username);
            if (targetUser == null)
            {
                ConsoleManager.WriteLineColored($"User '{username}' not found.", ConsoleStyle.Colors.Error);
                return;
            }
            
            if (username.Equals(Kernel.CurrentUser.Username, StringComparison.OrdinalIgnoreCase) && 
                action == "revoke")
            {
                ConsoleManager.WriteLineColored("You cannot revoke your own administrator privileges.", 
                    ConsoleStyle.Colors.Error);
                return;
            }

            switch (action)
            {
                case "grant":
                    if (SetUserAdminStatus(targetUser, true))
                    {
                        ConsoleManager.WriteLineColored($"Administrator privileges granted to '{username}'.", 
                            ConsoleStyle.Colors.Success);
                        Log.WriteLog(LogPriority.Info, "UserAdmin", 
                            $"Administrator privileges granted to '{username}' by '{Kernel.CurrentUser.Username}'", 
                            Kernel.CurrentUser.Username);
                    }
                    break;
                
                case "revoke":
                    if (SetUserAdminStatus(targetUser, false))
                    {
                        ConsoleManager.WriteLineColored($"Administrator privileges revoked from '{username}'.", 
                            ConsoleStyle.Colors.Success);
                        Log.WriteLog(LogPriority.Info, "UserAdmin", 
                            $"Administrator privileges revoked from '{username}' by '{Kernel.CurrentUser.Username}'", 
                            Kernel.CurrentUser.Username);
                    }
                    break;
                
                default:
                    ShowUsage();
                    break;
            }
        }

        private bool SetUserAdminStatus(User user, bool isAdmin)
        {
            try
            {
                string newType = isAdmin ? UserType.Administrator : UserType.Standard;
                
                user.Type = newType;
                
                UserManager.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Failed to update user privileges: {ex.Message}", 
                    ConsoleStyle.Colors.Error);
                return false;
            }
        }

        private void ShowUsage()
        {
            ConsoleManager.WriteLineColored("Usage:", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored("  useradmin grant <username>  - Grant administrator privileges", 
                ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored("  useradmin revoke <username> - Revoke administrator privileges", 
                ConsoleStyle.Colors.Primary);
        }
    }
}