using System;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Users
{
    public class UserInfoCommand : ICommand
    {
        public void Execute(string[] args)
        {
            string username;
            
            if (args.Length == 0)
            {
                if (Kernel.CurrentUser == null)
                {
                    ConsoleManager.WriteLineColored("No user is currently logged in.", ConsoleStyle.Colors.Warning);
                    return;
                }
                
                username = Kernel.CurrentUser.Username;
            }
            else
            {
                username = args[0];
                
                if (!username.Equals(Kernel.CurrentUser?.Username, StringComparison.OrdinalIgnoreCase) &&
                    (Kernel.CurrentUser == null || !Kernel.CurrentUser.IsAdministrator()))
                {
                    ConsoleManager.WriteLineColored("You can only view your own user information.", 
                        ConsoleStyle.Colors.Error);
                    return;
                }
            }
            
            User user = UserManager.GetUser(username);
            if (user == null)
            {
                ConsoleManager.WriteLineColored($"User '{username}' not found.", ConsoleStyle.Colors.Error);
                return;
            }
            
            ConsoleManager.WriteLineColored($"User Information:", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteColored("Username: ", ConsoleStyle.Colors.Primary);
            Console.WriteLine(user.Username);
            
            ConsoleManager.WriteColored("Type: ", ConsoleStyle.Colors.Primary);
            if (user.IsAdministrator())
            {
                ConsoleManager.WriteLineColored("Administrator", ConsoleStyle.Colors.Warning);
            }
            else
            {
                Console.WriteLine("Standard User");
            }
            
            ConsoleManager.WriteColored("Home Directory: ", ConsoleStyle.Colors.Primary);
            Console.WriteLine(user.HomeDirectory);
            
            ConsoleManager.WriteColored("Password Status: ", ConsoleStyle.Colors.Primary);
            if (user.PasswordExpired)
            {
                ConsoleManager.WriteLineColored("Password expired", ConsoleStyle.Colors.Error);
            }
            else
            {
                TimeSpan passwordAge = System.DateTime.Now - user.PasswordLastChanged;
                int daysLeft = UserManager.DefaultPasswordExpirationDays - (int)passwordAge.TotalDays;
                if (daysLeft < 14)
                {
                    ConsoleManager.WriteLineColored($"Password expires in {daysLeft} days", ConsoleStyle.Colors.Warning);
                }
                else
                {
                    ConsoleManager.WriteLineColored("Password active", ConsoleStyle.Colors.Success);
                }
            }
        }
    }
}