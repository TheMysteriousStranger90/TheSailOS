using System;
using TheSailOSProject.Logging;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Users;

public class CreateUserCommand : ICommand
{
    public void Execute(string[] args)
    {
        if (Kernel.CurrentUser == null || !Kernel.CurrentUser.IsAdministrator())
        {
            ConsoleManager.WriteLineColored("This command requires administrator privileges.",
                ConsoleStyle.Colors.Error);
            Log.WriteLog(LogPriority.Warning, "CreateUser",
                "Unauthorized access attempt",
                Kernel.CurrentUser?.Username ?? "Unknown");
            return;
        }

        if (args.Length < 2)
        {
            ShowUsage();
            return;
        }

        string username = args[0];
        string password = args[1];
        string userType = args.Length >= 3 ? args[2] : UserType.Standard;

        if (userType.ToLower() == "admin" || userType.ToLower() == "administrator")
        {
            userType = UserType.Administrator;
        }
        else
        {
            userType = UserType.Standard;
        }

        if (UserManager.CreateUser(username, password, userType))
        {
            ConsoleManager.WriteLineColored($"User '{username}' created successfully.",
                ConsoleStyle.Colors.Success);
            ConsoleManager.WriteLineColored($"Type: {userType}", ConsoleStyle.Colors.Primary);

            Log.WriteLog(LogPriority.Info, "CreateUser",
                $"User '{username}' created with type '{userType}' by '{Kernel.CurrentUser.Username}'",
                Kernel.CurrentUser.Username);
        }
        else
        {
            Log.WriteLog(LogPriority.Warning, "CreateUser",
                $"Failed to create user '{username}'",
                Kernel.CurrentUser.Username);
        }
    }

    private void ShowUsage()
    {
        ConsoleManager.WriteLineColored("\nCreate User Command", ConsoleStyle.Colors.Primary);
        ConsoleManager.WriteLineColored("Usage:", ConsoleStyle.Colors.Primary);
        ConsoleManager.WriteLineColored("  createuser <username> <password> [type]",
            ConsoleStyle.Colors.Primary);
        ConsoleManager.WriteLineColored("\nParameters:", ConsoleStyle.Colors.Primary);
        ConsoleManager.WriteLineColored("  username - Username (3-20 characters, alphanumeric, _, -)",
            ConsoleStyle.Colors.Primary);
        ConsoleManager.WriteLineColored("  password - Password (minimum 4 characters)",
            ConsoleStyle.Colors.Primary);
        ConsoleManager.WriteLineColored("  type     - User type: 'standard' or 'admin' (default: standard)",
            ConsoleStyle.Colors.Primary);
        Console.WriteLine();
    }
}