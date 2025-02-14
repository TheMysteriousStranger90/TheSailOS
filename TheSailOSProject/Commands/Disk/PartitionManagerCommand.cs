using System;
using TheSailOSProject.FileSystem;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Disk
{
    public class PartitionManagerCommand : ICommand
    {
        private readonly IDiskManager _diskManager;

        public PartitionManagerCommand(IDiskManager diskManager)
        {
            _diskManager = diskManager;
        }

        public void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                ShowHelp();
                return;
            }

            string action = args[0].ToLower();
            string partition = args[1];

            switch (action)
            {
                case "mount":
                    MountPartition(partition);
                    break;
                case "unmount":
                    UnmountPartition(partition);
                    break;
                case "label":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Usage: partition label <partition> <new_label>");
                        return;
                    }
                    SetLabel(partition, args[2]);
                    break;
                case "info":
                    ShowPartitionInfo(partition);
                    break;
                default:
                    ShowHelp();
                    break;
            }
        }

        private void MountPartition(string partition)
        {
            try
            {
                ConsoleManager.WriteLineColored($"Mounting partition {partition}...", ConsoleStyle.Colors.Primary);
                _diskManager.MountPartition(partition);
                ConsoleManager.WriteLineColored("Partition mounted successfully.", ConsoleStyle.Colors.Success);
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error mounting partition: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }

        private void UnmountPartition(string partition)
        {
            try
            {
                ConsoleManager.WriteLineColored($"Unmounting partition {partition}...", ConsoleStyle.Colors.Primary);
                _diskManager.UnmountPartition(partition);
                ConsoleManager.WriteLineColored("Partition unmounted successfully.", ConsoleStyle.Colors.Success);
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error unmounting partition: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }

        private void SetLabel(string partition, string label)
        {
            try
            {
                ConsoleManager.WriteLineColored($"Setting label for partition {partition}...", ConsoleStyle.Colors.Primary);
                _diskManager.SetPartitionLabel(partition, label);
                ConsoleManager.WriteLineColored("Label set successfully.", ConsoleStyle.Colors.Success);
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error setting label: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }

        private void ShowPartitionInfo(string partition)
        {
            _diskManager.ListPartitions(partition);
        }

        private void ShowHelp()
        {
            Console.WriteLine("Partition Manager Commands:");
            Console.WriteLine("  partition mount <partition>        - Mount a partition");
            Console.WriteLine("  partition unmount <partition>      - Unmount a partition");
            Console.WriteLine("  partition label <partition> <label> - Set partition label");
            Console.WriteLine("  partition info <partition>         - Show partition information");
        }
    }
}