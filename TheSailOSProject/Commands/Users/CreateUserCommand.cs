using System;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Users
{
    public class CreateUserCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length != 2)
            {
                ConsoleManager.WriteLineColored("Usage: createuser <username> <password>", ConsoleStyle.Colors.Error);
                return;
            }

            string username = args[0];
            string password = args[1];
            string type = UserType.Standard;

            if (UserManager.CreateUser(username, password, type))
            {
                ConsoleManager.WriteLineColored($"User {username} created successfully.", ConsoleStyle.Colors.Success);
            }
            else
            {
                ConsoleManager.WriteLineColored($"Failed to create user {username}.", ConsoleStyle.Colors.Error);
            }
        }

        public string HelpText()
        {
            return "Creates a new user.\nUsage: createuser <username> <password>";
        }
    }
}