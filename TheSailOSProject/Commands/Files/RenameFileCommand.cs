using System;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Files;

public class RenameFileCommand : ICommand
{
    private readonly IFileManager _fileManager;
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public RenameFileCommand(IFileManager fileManager, ICurrentDirectoryManager currentDirectoryManager)
    {
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
    }

    public void Execute(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: rename <oldPath> <newName>");
            return;
        }

        string sourcePath = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);
        string newName = args[1];

        try
        {
            _currentDirectoryManager.ValidatePath(sourcePath);
            _fileManager.RenameFile(sourcePath, newName);
            Console.WriteLine($"Renamed {sourcePath} to {newName}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error renaming: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error renaming: {ex.Message}");
        }
    }
}