using System;
using System.Collections.Generic;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Users;

public class ListUsersCommand : ICommand
{
    public void Execute(string[] args)
    {
        if (Kernel.CurrentUser == null || !Kernel.CurrentUser.IsAdministrator())
        {
            ConsoleManager.WriteLineColored("This command requires administrator privileges.",
                ConsoleStyle.Colors.Error);
            return;
        }

        List<User> users = UserManager.GetAllUsers();

        if (users.Count == 0)
        {
            ConsoleManager.WriteLineColored("No users found.", ConsoleStyle.Colors.Warning);
            return;
        }

        ConsoleManager.WriteLineColored($"\nRegistered Users ({users.Count} total):",
            ConsoleStyle.Colors.Primary);
        Console.WriteLine(new string('=', 80));

        Console.WriteLine(string.Format("  {0,-20} {1,-18} {2,-18} {3}",
            "Username", "Type", "Password", "Last Changed"));
        Console.WriteLine(new string('-', 80));

        foreach (var user in users)
        {
            string passwordStatus;
            ConsoleColor statusColor;

            if (user.PasswordExpired)
            {
                passwordStatus = "EXPIRED";
                statusColor = ConsoleColor.Red;
            }
            else if (user.PasswordLastChanged == System.DateTime.MinValue)
            {
                passwordStatus = "Never Changed";
                statusColor = ConsoleColor.Yellow;
            }
            else
            {
                TimeSpan age = System.DateTime.Now - user.PasswordLastChanged;
                int daysLeft = UserManager.DefaultPasswordExpirationDays - (int)age.TotalDays;

                if (daysLeft < 14)
                {
                    passwordStatus = $"Expires in {daysLeft}d";
                    statusColor = ConsoleColor.Yellow;
                }
                else
                {
                    passwordStatus = "Active";
                    statusColor = ConsoleColor.Green;
                }
            }

            string lastChanged = user.PasswordLastChanged != System.DateTime.MinValue
                ? user.PasswordLastChanged.ToString("yyyy-MM-dd")
                : "Never";

            bool isCurrentUser = user.Username.Equals(
                Kernel.CurrentUser?.Username,
                StringComparison.OrdinalIgnoreCase
            );

            if (isCurrentUser)
            {
                ConsoleManager.WriteColored("> ", ConsoleStyle.Colors.Success);
            }
            else
            {
                Console.Write("  ");
            }

            Console.Write(string.Format("{0,-20} ", user.Username));

            if (user.IsAdministrator())
            {
                ConsoleManager.WriteColored("Administrator", ConsoleStyle.Colors.Warning);
                Console.Write(new string(' ', 18 - "Administrator".Length));
            }
            else
            {
                Console.Write(string.Format("{0,-18} ", "Standard"));
            }

            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = statusColor;
            Console.Write(string.Format("{0,-18} ", passwordStatus));
            Console.ForegroundColor = oldColor;

            Console.WriteLine(lastChanged);
        }

        Console.WriteLine(new string('=', 80));
        Console.WriteLine();
    }
}