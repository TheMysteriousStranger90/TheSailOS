using System;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Directories;

public class RenameDirectoryCommand : ICommand
{
    private readonly IDirectoryManager _directoryManager;
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public RenameDirectoryCommand(IDirectoryManager directoryManager, ICurrentDirectoryManager currentDirectoryManager)
    {
        _directoryManager = directoryManager ?? throw new ArgumentNullException(nameof(directoryManager));
        _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
    }

    public void Execute(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: renamedir <path> <newName>");
            return;
        }

        string sourcePath = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);
        string newName = args[1];

        try
        {
            _currentDirectoryManager.ValidatePath(sourcePath);
            _directoryManager.RenameDirectory(sourcePath, newName);
            Console.WriteLine($"Renamed directory from {sourcePath} to {newName}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error renaming directory: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error renaming directory: {ex.Message}");
        }
    }
}