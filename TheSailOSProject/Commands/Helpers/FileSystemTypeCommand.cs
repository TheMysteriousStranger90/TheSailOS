using System;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Helpers;

public class FileSystemTypeCommand : ICommand
{
    private readonly IVFSManager _vfsManager;

    public FileSystemTypeCommand(IVFSManager vfsManager)
    {
        _vfsManager = vfsManager ?? throw new ArgumentNullException(nameof(vfsManager));
    }

    public void Execute(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: fstype <drive>");
            return;
        }

        string drive = args[0];

        try
        {
            string fileSystemType = _vfsManager.GetFileSystemTypeTheSail(drive);
            Console.WriteLine($"File system type on drive {drive}: {fileSystemType}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting file system type: {ex.Message}");
        }
    }
}