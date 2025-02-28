using System;
using TheSailOSProject.Logging;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Logging
{
    public class LogCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }
            
            if (args[0].ToLower() == "admin" || args[0].ToLower() == "clear")
            {
                HandleAdminCommands(args);
                return;
            }
            
            int priority = LogPriority.Info;
            string message = string.Join(" ", args);
            string username = Kernel.CurrentUser?.Username ?? "SYSTEM";
            
            Log.WriteLog(priority, "UserCommand", message, username);
            ConsoleManager.WriteLineColored("Message logged successfully.", ConsoleStyle.Colors.Success);
        }
        
        private void HandleAdminCommands(string[] args)
        {
            if (Kernel.CurrentUser == null || Kernel.CurrentUser.Type != UserType.Administrator)
            {
                ConsoleManager.WriteLineColored("Insufficient permissions to manage logs.", ConsoleStyle.Colors.Error);
                return;
            }
            
            if (args[0].ToLower() == "clear")
            {
                Log.Clear();
                ConsoleManager.WriteLineColored("Log file cleared.", ConsoleStyle.Colors.Success);
                return;
            }
            
            if (args[0].ToLower() == "admin")
            {
                int count = 20;
                int? minPriority = null;
                
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i].ToLower() == "-n" && i + 1 < args.Length && int.TryParse(args[i + 1], out int parsedCount))
                    {
                        count = parsedCount;
                        i++;
                    }
                    else
                    {
                        switch (args[i].ToLower())
                        {
                            case "debug":
                                minPriority = LogPriority.Debug;
                                break;
                            case "info":
                                minPriority = LogPriority.Info;
                                break;
                            case "warning":
                                minPriority = LogPriority.Warning;
                                break;
                            case "error":
                                minPriority = LogPriority.Error;
                                break;
                            case "critical":
                                minPriority = LogPriority.Critical;
                                break;
                        }
                    }
                }
                
                DisplayLogs(count, minPriority);
            }
        }
        
        private void DisplayLogs(int count, int? minPriority)
        {
            var logEntries = Log.ReadLog(count, minPriority);
            
            if (logEntries.Count == 0)
            {
                ConsoleManager.WriteLineColored("No log entries found.", ConsoleStyle.Colors.Warning);
                return;
            }
            
            ConsoleManager.WriteLineColored($"Showing {logEntries.Count} log entries:", ConsoleStyle.Colors.Primary);
            Console.WriteLine();
            
            foreach (var entry in logEntries)
            {
                ConsoleColor color = entry.Priority switch
                {
                    0 => ConsoleColor.Gray,
                    1 => ConsoleColor.White,
                    2 => ConsoleColor.Yellow,
                    3 => ConsoleColor.Red,
                    4 => ConsoleColor.DarkRed,
                    _ => ConsoleColor.White
                };
                
                Console.ForegroundColor = color;
                Console.WriteLine(entry.ToString());
            }
            
            Console.ResetColor();
        }
        
        private void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  log <message>           - Log a message");
            Console.WriteLine("  log admin [options]      - View logs (admin only)");
            Console.WriteLine("  log clear               - Clear logs (admin only)");
            Console.WriteLine("Options:");
            Console.WriteLine("  -n <count>              - Number of entries to show");
            Console.WriteLine("  debug|info|warning|error|critical - Minimum priority level");
        }
    }
}