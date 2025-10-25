using System;
using TheSailOSProject.Logging;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Users;

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
                Log.WriteLog(LogPriority.Warning, "UserInfo",
                    $"Unauthorized attempt to view user info for '{username}'",
                    Kernel.CurrentUser?.Username ?? "Unknown");
                return;
            }
        }

        User user = UserManager.GetUser(username);
        if (user == null)
        {
            ConsoleManager.WriteLineColored($"User '{username}' not found.", ConsoleStyle.Colors.Error);
            return;
        }

        ConsoleManager.WriteLineColored($"\nUser Information for '{user.Username}':", ConsoleStyle.Colors.Primary);
        Console.WriteLine(new string('-', 50));

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
            ConsoleManager.WriteLineColored("Expired - Change required", ConsoleStyle.Colors.Error);
        }
        else if (user.PasswordLastChanged == System.DateTime.MinValue)
        {
            ConsoleManager.WriteLineColored("Never changed - Change recommended", ConsoleStyle.Colors.Warning);
        }
        else
        {
            TimeSpan passwordAge = System.DateTime.Now - user.PasswordLastChanged;
            int daysLeft = UserManager.DefaultPasswordExpirationDays - (int)passwordAge.TotalDays;

            ConsoleManager.WriteColored("Active ", ConsoleStyle.Colors.Success);
            Console.Write($"(expires in {daysLeft} days");

            if (daysLeft < 14)
            {
                ConsoleManager.WriteColored(" - WARNING", ConsoleStyle.Colors.Warning);
            }

            Console.WriteLine(")");
        }

        ConsoleManager.WriteColored("Password Last Changed: ", ConsoleStyle.Colors.Primary);
        if (user.PasswordLastChanged != System.DateTime.MinValue)
        {
            Console.WriteLine(user.PasswordLastChanged.ToString());
        }
        else
        {
            ConsoleManager.WriteLineColored("Never", ConsoleStyle.Colors.Warning);
        }

        Console.WriteLine(new string('-', 50));
        Console.WriteLine();
    }
}