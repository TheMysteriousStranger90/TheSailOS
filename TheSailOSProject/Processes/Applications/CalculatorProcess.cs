using System;
using TheSailOSProject.Applications;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Processes.Applications;

public class CalculatorProcess : Process
{
    public CalculatorProcess() : base("Calculator", ProcessType.Application)
    {
    }

    public override void Start()
    {
        base.Start();
        Console.Clear();
        ConsoleManager.WriteLineColored("Starting Calculator...", ConsoleStyle.Colors.Primary);
        ConsoleManager.WriteLineColored("Enter an expression (e.g., 1+2*3) or type 'exit' to close",
            ConsoleStyle.Colors.Accent);
        ConsoleManager.WriteLineColored("Press any key to start...", ConsoleStyle.Colors.Primary);
        Console.ReadKey(true);
    }

    public override void Run()
    {
        Calculator.Run();
        Stop();
    }

    public override void Stop()
    {
        base.Stop();
        Console.Clear();
        ConsoleManager.WriteLineColored("[Calculator] Closed", ConsoleStyle.Colors.Primary);
    }
}