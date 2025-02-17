using System;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Users;

public class LoginCommand : ICommand
{
    private readonly ILoginHandler _loginHandler;

    public LoginCommand(ILoginHandler loginHandler)
    {
        _loginHandler = loginHandler ?? throw new ArgumentNullException(nameof(loginHandler));
    }

    public void Execute(string[] args)
    {
        if (args.Length != 2)
        {
            ConsoleManager.WriteLineColored("Usage: login <username> <password>", ConsoleStyle.Colors.Error);
            return;
        }

        string username = args[0];
        string password = args[1];

        if (UserManager.VerifyLogin(username, password, out User loggedInUser))
        {
            ConsoleManager.WriteLineColored($"Login successful for user: {username}", ConsoleStyle.Colors.Success);
            _loginHandler.OnLoginSuccess(loggedInUser);
        }
        else
        {
            ConsoleManager.WriteLineColored("Login failed. Invalid username or password.", ConsoleStyle.Colors.Error);
        }
    }

    public string HelpText()
    {
        return "Logs in a user.\nUsage: login <username> <password>";
    }
}