using TheSailOSProject.Hardware.Timer;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.SystemTimer
{
    public class TimerCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            string subCommand = args[0].ToLower();

            switch (subCommand)
            {
                case "list":
                    TimerManager.ListTimers();
                    break;

                case "create":
                    HandleCreateTimer(args);
                    break;

                case "destroy":
                    HandleDestroyTimer(args);
                    break;

                case "help":
                    ShowHelp();
                    break;

                default:
                    ConsoleManager.WriteLineColored($"Unknown timer command: {subCommand}", ConsoleStyle.Colors.Error);
                    ShowHelp();
                    break;
            }
        }

        private void HandleCreateTimer(string[] args)
        {
            if (args.Length < 4)
            {
                ConsoleManager.WriteLineColored("Invalid parameters for timer create command.",
                    ConsoleStyle.Colors.Error);
                ConsoleManager.WriteLineColored("Usage: timer create <name> <interval_ms> <recurring>",
                    ConsoleStyle.Colors.Warning);
                return;
            }

            string name = args[1];

            if (!ulong.TryParse(args[2], out ulong intervalMs))
            {
                ConsoleManager.WriteLineColored("Invalid interval. Please specify a valid number.",
                    ConsoleStyle.Colors.Error);
                return;
            }

            ulong intervalNs = intervalMs * 1000000;

            if (!bool.TryParse(args[3], out bool recurring))
            {
                ConsoleManager.WriteLineColored("Invalid recurring value. Use 'true' or 'false'.",
                    ConsoleStyle.Colors.Error);
                return;
            }

            bool success = TimerManager.CreateTimer(name, intervalNs,
                () => { ConsoleManager.WriteLineColored($"Timer '{name}' triggered!", ConsoleStyle.Colors.Secondary); },
                recurring);

            if (success)
            {
                ConsoleManager.WriteLineColored($"Timer '{name}' created successfully.", ConsoleStyle.Colors.Success);
            }
        }

        private void HandleDestroyTimer(string[] args)
        {
            if (args.Length < 2)
            {
                ConsoleManager.WriteLineColored("Please specify a timer name to destroy.", ConsoleStyle.Colors.Error);
                return;
            }

            string name = args[1];
            bool success = TimerManager.DestroyTimer(name);

            if (success)
            {
                ConsoleManager.WriteLineColored($"Timer '{name}' destroyed successfully.", ConsoleStyle.Colors.Success);
            }
        }

        private void ShowHelp()
        {
            ConsoleManager.WriteLineColored("Timer Command Usage:", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored("  timer list                          - List all active timers",
                ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored("  timer create <name> <ms> <repeat>   - Create a new timer",
                ConsoleStyle.Colors.Secondary);
            ConsoleManager.WriteLineColored("  timer destroy <name>                - Destroy an existing timer",
                ConsoleStyle.Colors.Secondary);
            ConsoleManager.WriteLineColored("  timer help                          - Show this help",
                ConsoleStyle.Colors.Secondary);
            ConsoleManager.WriteLineColored("\nExamples:", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored("  timer create reminder 5000 true     - Create a 5-second recurring timer",
                ConsoleStyle.Colors.Secondary);
            ConsoleManager.WriteLineColored("  timer create countdown 10000 false  - Create a one-time 10-second timer",
                ConsoleStyle.Colors.Secondary);
        }
    }
}