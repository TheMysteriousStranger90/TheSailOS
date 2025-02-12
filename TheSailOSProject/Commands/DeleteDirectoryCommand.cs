using System;
using System.IO;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands;

public class DeleteDirectoryCommand : ICommand
{
    private readonly IDirectoryManager _directoryManager; // Add this
    private readonly ICurrentDirectoryManager _currentDirectoryManager;
    private readonly IRootDirectoryProvider _rootDirectoryProvider;

    public DeleteDirectoryCommand(IDirectoryManager directoryManager, ICurrentDirectoryManager currentDirectoryManager, IRootDirectoryProvider rootDirectoryProvider) // Modify constructor
    {
        _directoryManager = directoryManager ?? throw new ArgumentNullException(nameof(directoryManager)); // Add this
        _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
        _rootDirectoryProvider = rootDirectoryProvider ?? throw new ArgumentNullException(nameof(rootDirectoryProvider));
    }

    public void Execute(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: rmdir <directory>");
            return;
        }

        var directory = args[0];
        var currentDirectory = _currentDirectoryManager.GetCurrentDirectory();
        var rootDirectory = _rootDirectoryProvider.GetRootDirectory();
        var path = _currentDirectoryManager.CombinePath(currentDirectory, directory);

        try
        {
            if (path == rootDirectory)
            {
                Console.WriteLine("Cannot delete root directory");
                return;
            }

            _currentDirectoryManager.ValidatePath(path);
            _directoryManager.DeleteDirectory(path);
            Console.WriteLine($"Directory deleted: {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting directory: {ex.Message}");
        }
    }
}