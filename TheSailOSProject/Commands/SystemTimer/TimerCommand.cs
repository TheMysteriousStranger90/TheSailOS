using System;
using TheSailOSProject.Hardware.Timer;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.SystemTimer;

public class TimerCommand : ICommand
{
    public string Name => "timer";
    public string Description => "Manage system timers";

    public void Execute(string[] args)
    {
        if (args.Length < 1)
        {
            ShowUsage();
            return;
        }

        string action = args[0].ToLower();

        switch (action)
        {
            case "list":
            case "ls":
                TimerManager.ListTimers();
                break;

            case "info":
                if (args.Length < 2)
                {
                    ConsoleManager.WriteLineColored("Usage: timer info <name>",
                        ConsoleStyle.Colors.Error);
                    return;
                }

                TimerManager.ShowTimerInfo(args[1]);
                break;

            case "enable":
                if (args.Length < 2)
                {
                    ConsoleManager.WriteLineColored("Usage: timer enable <name>",
                        ConsoleStyle.Colors.Error);
                    return;
                }

                if (TimerManager.EnableTimer(args[1]))
                {
                    ConsoleManager.WriteLineColored($"Timer '{args[1]}' enabled",
                        ConsoleStyle.Colors.Success);
                }
                else
                {
                    ConsoleManager.WriteLineColored($"Timer '{args[1]}' not found",
                        ConsoleStyle.Colors.Error);
                }

                break;

            case "disable":
                if (args.Length < 2)
                {
                    ConsoleManager.WriteLineColored("Usage: timer disable <name>",
                        ConsoleStyle.Colors.Error);
                    return;
                }

                if (TimerManager.DisableTimer(args[1]))
                {
                    ConsoleManager.WriteLineColored($"Timer '{args[1]}' disabled",
                        ConsoleStyle.Colors.Success);
                }
                else
                {
                    ConsoleManager.WriteLineColored($"Timer '{args[1]}' not found",
                        ConsoleStyle.Colors.Error);
                }

                break;

            case "stats":
                ShowStats();
                break;

            default:
                ShowUsage();
                break;
        }
    }

    private void ShowUsage()
    {
        ConsoleManager.WriteLineColored("Timer Manager", ConsoleStyle.Colors.Primary);
        Console.WriteLine("Usage:");
        Console.WriteLine("  timer list               - List all timers");
        Console.WriteLine("  timer info <name>        - Show timer details");
        Console.WriteLine("  timer enable <name>      - Enable a timer");
        Console.WriteLine("  timer disable <name>     - Disable a timer");
        Console.WriteLine("  timer stats              - Show timer statistics");
    }

    private void ShowStats()
    {
        int count = TimerManager.GetTimerCount();

        ConsoleManager.WriteLineColored("\nTimer Statistics:", ConsoleStyle.Colors.Primary);
        Console.WriteLine(new string('=', 50));

        ConsoleManager.WriteColored("  Total Timers:  ", ConsoleStyle.Colors.Primary);
        Console.WriteLine(count);

        Console.WriteLine(new string('=', 50));
    }
}