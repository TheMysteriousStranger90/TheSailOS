using System;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Disk;

public class CreatePartitionCommand : ICommand
{
    private readonly IDiskManager _diskManager;

    public CreatePartitionCommand(IDiskManager diskManager)
    {
        _diskManager = diskManager;
    }

    public void Execute(string[] args)
    {
        if (args.Length != 2 || !int.TryParse(args[1], out int size))
        {
            Console.WriteLine("Usage: partition <drive_letter> <size>");
            return;
        }

        _diskManager.CreatePartition(args[0], size);
    }
}