using System;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Users;

public class LogoutCommand : ICommand
{
    private readonly ILogoutHandler _logoutHandler;

    public LogoutCommand(ILogoutHandler logoutHandler)
    {
        _logoutHandler = logoutHandler ?? throw new ArgumentNullException(nameof(logoutHandler));
    }

    public void Execute(string[] args)
    {
        _logoutHandler.OnLogout();
    }

    public string HelpText()
    {
        return "Logs out the current user.\nUsage: logout";
    }
}