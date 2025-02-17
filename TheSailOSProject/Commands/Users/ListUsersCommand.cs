using System;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Users
{
    public class ListUsersCommand : ICommand
    {
        public void Execute(string[] args)
        {
            var users = UserManager.GetAllUsers();

            if (users.Count == 0)
            {
                ConsoleManager.WriteLineColored("No users found.", ConsoleStyle.Colors.Warning);
                return;
            }

            ConsoleManager.WriteLineColored("Users:", ConsoleStyle.Colors.Primary);
            foreach (var user in users)
            {
                ConsoleManager.WriteLineColored($"- {user.Username} ({user.Type})", ConsoleStyle.Colors.Primary);
            }
        }

        public string HelpText()
        {
            return "Lists all users.\nUsage: listusers";
        }
    }
}