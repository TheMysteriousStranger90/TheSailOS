using System;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Disk;

public class FormatDriveCommand : ICommand
{
    private readonly IDiskManager _diskManager;

    public FormatDriveCommand(IDiskManager diskManager)
    {
        _diskManager = diskManager;
    }

    public void Execute(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: format <drive_letter>");
            return;
        }

        _diskManager.FormatDrive(args[0]);
    }
}