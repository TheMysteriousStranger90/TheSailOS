using System;
using TheSailOSProject.Processes;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Processes;

public class ProcessCommand : ICommand
{
    public string Name => "process";
    public string Description => "Manage system processes";

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
                ListProcesses();
                break;

            case "info":
                if (args.Length < 2)
                {
                    ConsoleManager.WriteLineColored("Usage: process info <name|id>", 
                        ConsoleStyle.Colors.Error);
                    return;
                }
                ShowProcessInfo(args[1]);
                break;

            case "stop":
                if (args.Length < 2)
                {
                    ConsoleManager.WriteLineColored("Usage: process stop <name|id>", 
                        ConsoleStyle.Colors.Error);
                    return;
                }
                StopProcess(args[1]);
                break;

            case "start":
                if (args.Length < 2)
                {
                    ConsoleManager.WriteLineColored("Usage: process start <name|id>", 
                        ConsoleStyle.Colors.Error);
                    return;
                }
                StartProcess(args[1]);
                break;

            default:
                ShowUsage();
                break;
        }
    }

    private void ShowUsage()
    {
        ConsoleManager.WriteLineColored("Process Manager", ConsoleStyle.Colors.Primary);
        Console.WriteLine("Usage:");
        Console.WriteLine("  process list              - List all processes");
        Console.WriteLine("  process info <name|id>    - Show process details");
        Console.WriteLine("  process stop <name|id>    - Stop a process");
        Console.WriteLine("  process start <name|id>   - Start a process");
    }

    private void ListProcesses()
    {
        var processes = ProcessManager.GetProcesses();

        if (processes.Count == 0)
        {
            ConsoleManager.WriteLineColored("No processes running", ConsoleStyle.Colors.Warning);
            return;
        }

        ConsoleManager.WriteLineColored($"\nSystem Processes ({processes.Count}):", 
            ConsoleStyle.Colors.Primary);
        Console.WriteLine(new string('=', 80));
        Console.WriteLine(string.Format("  {0,-5} {1,-30} {2,-12} {3}", 
            "ID", "Name", "Status", "Type"));
        Console.WriteLine(new string('-', 80));

        foreach (var process in processes)
        {
            string status = process.IsRunning ? "Running" : "Stopped";
            var statusColor = process.IsRunning ? ConsoleColor.Green : ConsoleColor.Yellow;

            Console.Write("  ");
            Console.Write(string.Format("{0,-5} ", process.Id));
            Console.Write(string.Format("{0,-30} ", process.Name));

            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = statusColor;
            Console.Write(string.Format("{0,-12} ", status));
            Console.ForegroundColor = oldColor;

            Console.WriteLine(process.Type);
        }

        Console.WriteLine(new string('=', 80));
    }

    private void ShowProcessInfo(string identifier)
    {
        Process process = FindProcess(identifier);
        if (process == null)
        {
            ConsoleManager.WriteLineColored($"Process '{identifier}' not found", 
                ConsoleStyle.Colors.Error);
            return;
        }

        ConsoleManager.WriteLineColored($"\nProcess Information: {process.Name}", 
            ConsoleStyle.Colors.Primary);
        Console.WriteLine(new string('=', 50));

        ConsoleManager.WriteColored("  ID:           ", ConsoleStyle.Colors.Primary);
        Console.WriteLine(process.Id);

        ConsoleManager.WriteColored("  Name:         ", ConsoleStyle.Colors.Primary);
        Console.WriteLine(process.Name);

        ConsoleManager.WriteColored("  Type:         ", ConsoleStyle.Colors.Primary);
        Console.WriteLine(process.Type);

        ConsoleManager.WriteColored("  Status:       ", ConsoleStyle.Colors.Primary);
        if (process.IsRunning)
        {
            ConsoleManager.WriteLineColored("Running", ConsoleStyle.Colors.Success);
        }
        else
        {
            ConsoleManager.WriteLineColored("Stopped", ConsoleStyle.Colors.Warning);
        }

        ConsoleManager.WriteColored("  Created:      ", ConsoleStyle.Colors.Primary);
        Console.WriteLine(process.Created);

        ConsoleManager.WriteColored("  Uptime:       ", ConsoleStyle.Colors.Primary);
        var uptime = System.DateTime.Now - process.Created;
        Console.WriteLine($"{uptime.TotalMinutes:F1} minutes");

        Console.WriteLine(new string('=', 50));
    }

    private void StopProcess(string identifier)
    {
        Process process = FindProcess(identifier);
        if (process == null)
        {
            ConsoleManager.WriteLineColored($"Process '{identifier}' not found", 
                ConsoleStyle.Colors.Error);
            return;
        }

        if (ProcessManager.Stop(process))
        {
            ConsoleManager.WriteLineColored($"Process '{process.Name}' stopped", 
                ConsoleStyle.Colors.Success);
        }
    }

    private void StartProcess(string identifier)
    {
        Process process = FindProcess(identifier);
        if (process == null)
        {
            ConsoleManager.WriteLineColored($"Process '{identifier}' not found", 
                ConsoleStyle.Colors.Error);
            return;
        }

        if (ProcessManager.Start(process))
        {
            ConsoleManager.WriteLineColored($"Process '{process.Name}' started", 
                ConsoleStyle.Colors.Success);
        }
    }

    private Process FindProcess(string identifier)
    {
        if (ulong.TryParse(identifier, out ulong id))
        {
            return ProcessManager.GetProcessById(id);
        }

        return ProcessManager.GetProcessByName(identifier);
    }
}