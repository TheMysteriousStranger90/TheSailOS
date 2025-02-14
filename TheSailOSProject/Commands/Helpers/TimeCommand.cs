using TheSailOSProject.DateTime;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Helpers;

public class TimeCommand : ICommand
{
    public void Execute(string[] args)
    {
        ConsoleManager.WriteColored("Current Time: ", ConsoleStyle.Colors.Primary);
        TheSailDateTime.ShowTime();
    }
}