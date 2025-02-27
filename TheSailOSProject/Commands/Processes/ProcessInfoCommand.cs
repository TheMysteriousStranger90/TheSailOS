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
            try
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
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error executing processinfo command: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }

        private void ListAllProcesses()
        {
            try
            {
                List<Process> processes = ProcessManager.GetProcesses();
                if (processes == null || processes.Count == 0)
                {
                    ConsoleManager.WriteLineColored("No processes running.", ConsoleStyle.Colors.Primary);
                    return;
                }

                ConsoleManager.WriteLineColored("Running Processes:", ConsoleStyle.Colors.Primary);
                
                foreach (var process in processes)
                {
                    if (process != null)
                    {
                        try
                        {
                            string processInfo = $"  ID: {process.Id}, Name: {SafeGetValue(() => process.Name, "Unknown")}, ";
                            processInfo += $"Type: {SafeGetValue(() => process.Type.ToString(), "Unknown")}, ";
                            processInfo += $"Running: {SafeGetValue(() => process.IsRunning.ToString(), "Unknown")}";
                            
                            Console.WriteLine(processInfo);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  [Error displaying process: {ex.Message}]");
                        }
                    }
                    else
                    {
                        Console.WriteLine("  [Invalid Process Entry]");
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error listing processes: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }

        private void ShowProcessInfoById(ulong id)
        {
            try
            {
                Process process = ProcessManager.GetProcessById(id);
                if (process == null)
                {
                    ConsoleManager.WriteLineColored($"Process with ID {id} not found.", ConsoleStyle.Colors.Error);
                    return;
                }

                DisplayProcessInfo(process);
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error getting process by ID: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }

        private void ShowProcessInfoByName(string name)
        {
            try
            {
                Process process = ProcessManager.GetProcessByName(name);
                if (process == null)
                {
                    ConsoleManager.WriteLineColored($"Process with name '{name}' not found.", ConsoleStyle.Colors.Error);
                    return;
                }

                DisplayProcessInfo(process);
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error getting process by name: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }

        private void DisplayProcessInfo(Process process)
        {
            try
            {
                ConsoleManager.WriteLineColored("Process Information:", ConsoleStyle.Colors.Primary);
                Console.WriteLine($"  ID: {process.Id}");
                Console.WriteLine($"  Name: {SafeGetValue(() => process.Name, "Unknown")}");
                Console.WriteLine($"  Type: {SafeGetValue(() => process.Type.ToString(), "Unknown")}");
                Console.WriteLine($"  Running: {SafeGetValue(() => process.IsRunning.ToString(), "Unknown")}");
                Console.WriteLine($"  Created: {SafeGetValue(() => process.Created.ToString(), "Unknown")}");
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error displaying process information: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }
        
        private string SafeGetValue(Func<string> getter, string defaultValue)
        {
            try
            {
                string value = getter();
                return !string.IsNullOrEmpty(value) ? value : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}