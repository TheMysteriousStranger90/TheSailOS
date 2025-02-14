using TheSailOSProject.DateTime;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Helpers;

public class DateCommand : ICommand
{
    public void Execute(string[] args)
    {
        ConsoleManager.WriteColored("Current Date: ", ConsoleStyle.Colors.Primary);
        TheSailDateTime.ShowDate();
    }
}