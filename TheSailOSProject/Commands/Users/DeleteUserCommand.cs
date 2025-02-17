using System;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Users
{
    public class DeleteUserCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length != 1)
            {
                ConsoleManager.WriteLineColored("Usage: deleteuser <username>", ConsoleStyle.Colors.Error);
                return;
            }

            string username = args[0];

            if (username.ToLower() == "admin")
            {
                ConsoleManager.WriteLineColored("Cannot delete the admin user.", ConsoleStyle.Colors.Error);
                return;
            }

            if (UserManager.DeleteUser(username))
            {
                ConsoleManager.WriteLineColored($"User {username} deleted successfully.", ConsoleStyle.Colors.Success);
            }
            else
            {
                ConsoleManager.WriteLineColored($"Failed to delete user {username}.", ConsoleStyle.Colors.Error);
            }
        }

        public string HelpText()
        {
            return "Deletes a user.\nUsage: deleteuser <username>";
        }
    }
}