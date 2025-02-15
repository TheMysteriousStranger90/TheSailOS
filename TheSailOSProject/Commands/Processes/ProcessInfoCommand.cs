using System;
using System.Collections.Generic;
using TheSailOSProject.Processes;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Processes
{
    public class ProcessInfoCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ListAllProcesses();
            }
            else if (args.Length == 2 && args[0].ToLower() == "id")
            {
                if (ulong.TryParse(args[1], out ulong id))
                {
                    ShowProcessInfoById(id);
                }
                else
                {
                    ConsoleManager.WriteLineColored("Invalid process ID.", ConsoleStyle.Colors.Error);
                }
            }
            else if (args.Length == 2 && args[0].ToLower() == "name")
            {
                ShowProcessInfoByName(args[1]);
            }
            else
            {
                ConsoleManager.WriteLineColored("Usage: processinfo [id <ID> | name <Name>]", ConsoleStyle.Colors.Warning);
            }
        }

        private void ListAllProcesses()
        {
            List<Process> processes = ProcessManager.GetProcesses();
            if (processes.Count == 0)
            {
                ConsoleManager.WriteLineColored("No processes running.", ConsoleStyle.Colors.Primary);
                return;
            }

            ConsoleManager.WriteLineColored("Running Processes:", ConsoleStyle.Colors.Primary);
            foreach (var process in processes)
            {
                Console.WriteLine($"  ID: {process.Id}, Name: {process.Name}, Type: {process.Type}, Running: {process.IsRunning}");
            }
        }

        private void ShowProcessInfoById(ulong id)
        {
            Process process = ProcessManager.GetProcessById(id);
            if (process == null)
            {
                ConsoleManager.WriteLineColored($"Process with ID {id} not found.", ConsoleStyle.Colors.Error);
                return;
            }

            DisplayProcessInfo(process);
        }

        private void ShowProcessInfoByName(string name)
        {
            Process process = ProcessManager.GetProcessByName(name);
            if (process == null)
            {
                ConsoleManager.WriteLineColored($"Process with name '{name}' not found.", ConsoleStyle.Colors.Error);
                return;
            }

            DisplayProcessInfo(process);
        }

        private void DisplayProcessInfo(Process process)
        {
            ConsoleManager.WriteLineColored("Process Information:", ConsoleStyle.Colors.Primary);
            Console.WriteLine($"  ID: {process.Id}");
            Console.WriteLine($"  Name: {process.Name}");
            Console.WriteLine($"  Type: {process.Type}");
            Console.WriteLine($"  Running: {process.IsRunning}");
            Console.WriteLine($"  Created: {process.Created}");
        }
    }
}