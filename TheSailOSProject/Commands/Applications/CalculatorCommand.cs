using System.Threading;
using TheSailOSProject.Processes;
using TheSailOSProject.Processes.Applications;

namespace TheSailOSProject.Commands.Applications;

public class CalculatorCommand : ICommand
{
    public void Execute(string[] args)
    {
        var calcProcess = new CalculatorProcess();
        ProcessManager.Register(calcProcess);
        ProcessManager.Start(calcProcess);

        while (calcProcess.IsRunning)
        {
            ProcessManager.Update();
            Thread.Sleep(100);
        }
    }

    public string HelpText()
    {
        return "Opens the calculator application.\nUsage: calculator";
    }
}