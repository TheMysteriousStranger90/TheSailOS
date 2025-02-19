using TheSailOSProject.Applications;

namespace TheSailOSProject.Commands.Applications;

public class CalculatorCommand : ICommand
{
    public void Execute(string[] args)
    {
        Calculator.Run();
    }

    public string HelpText()
    {
        return "Opens the calculator application.\nUsage: calculator";
    }
}