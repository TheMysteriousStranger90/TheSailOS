using System;
using TheSailOSProject.Logging;
using TheSailOSProject.Session;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Users;

public class DeleteUserCommand : ICommand
{
    public void Execute(string[] args)
    {
        if (Kernel.CurrentUser == null || !Kernel.CurrentUser.IsAdministrator())
        {
            ConsoleManager.WriteLineColored("This command requires administrator privileges.",
                ConsoleStyle.Colors.Error);
            Log.WriteLog(LogPriority.Warning, "DeleteUser",
                "Unauthorized access attempt",
                Kernel.CurrentUser?.Username ?? "Unknown");
            return;
        }

        if (args.Length < 1)
        {
            ShowUsage();
            return;
        }

        string username = args[0];

        if (username.Equals(Kernel.CurrentUser.Username, StringComparison.OrdinalIgnoreCase))
        {
            ConsoleManager.WriteLineColored("You cannot delete your own user account.",
                ConsoleStyle.Colors.Error);
            return;
        }

        if (username.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            ConsoleManager.WriteLineColored("Cannot delete the default admin user.",
                ConsoleStyle.Colors.Error);
            return;
        }

        User userToDelete = UserManager.GetUser(username);
        if (userToDelete == null)
        {
            ConsoleManager.WriteLineColored($"User '{username}' not found.", ConsoleStyle.Colors.Error);
            return;
        }

        ConsoleManager.WriteLineColored($"Are you sure you want to delete user '{username}'?",
            ConsoleStyle.Colors.Warning);
        ConsoleManager.WriteColored("Type 'yes' to confirm: ", ConsoleStyle.Colors.Warning);
        string confirmation = Console.ReadLine();

        if (confirmation != null && confirmation.Trim().ToLower() == "yes")
        {
            SessionManager.TerminateAllUserSessions(username);

            if (UserManager.DeleteUser(username))
            {
                ConsoleManager.WriteLineColored($"User '{username}' deleted successfully.",
                    ConsoleStyle.Colors.Success);

                Log.WriteLog(LogPriority.Info, "DeleteUser",
                    $"User '{username}' deleted by '{Kernel.CurrentUser.Username}'",
                    Kernel.CurrentUser.Username);
            }
            else
            {
                Log.WriteLog(LogPriority.Warning, "DeleteUser",
                    $"Failed to delete user '{username}'",
                    Kernel.CurrentUser.Username);
            }
        }
        else
        {
            ConsoleManager.WriteLineColored("User deletion cancelled.", ConsoleStyle.Colors.Primary);
        }
    }

    private void ShowUsage()
    {
        ConsoleManager.WriteLineColored("\nDelete User Command", ConsoleStyle.Colors.Primary);
        ConsoleManager.WriteLineColored("Usage:", ConsoleStyle.Colors.Primary);
        ConsoleManager.WriteLineColored("  deleteuser <username>", ConsoleStyle.Colors.Primary);
        ConsoleManager.WriteLineColored("\nNote: This will terminate all active sessions for the user.",
            ConsoleStyle.Colors.Warning);
        Console.WriteLine();
    }
}