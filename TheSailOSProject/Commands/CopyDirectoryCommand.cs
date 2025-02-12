using System;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands;

public class CopyDirectoryCommand : ICommand
{
    private readonly IDirectoryManager _directoryManager;
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public CopyDirectoryCommand(IDirectoryManager directoryManager, ICurrentDirectoryManager currentDirectoryManager)
    {
        _directoryManager = directoryManager ?? throw new ArgumentNullException(nameof(directoryManager));
        _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
    }

    public void Execute(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: copydir <source> <destination>");
            return;
        }

        string sourcePath = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);
        string destinationPath = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[1]);

        try
        {
            _currentDirectoryManager.ValidatePath(sourcePath);
            _currentDirectoryManager.ValidatePath(destinationPath);
            _directoryManager.CopyDirectory(sourcePath, destinationPath);
            Console.WriteLine($"Copied directory from {sourcePath} to {destinationPath}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error copying directory: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copying directory: {ex.Message}");
        }
    }
}