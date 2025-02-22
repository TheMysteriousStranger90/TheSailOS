using System;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Helpers;

public class FreeSpaceCommand : ICommand
{
    private readonly IVFSManager _vfsManager;

    public FreeSpaceCommand(IVFSManager vfsManager)
    {
        _vfsManager = vfsManager ?? throw new ArgumentNullException(nameof(vfsManager));
    }

    public void Execute(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: freespace <drive>");
            return;
        }

        string drive = args[0];

        try
        {
            long freeSpace = _vfsManager.GetAvailableFreeSpaceTheSail(drive);
            Console.WriteLine($"Available free space on drive {drive}: {freeSpace} bytes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting free space: {ex.Message}");
        }
    }
}