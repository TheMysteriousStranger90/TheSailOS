using System;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Files;

public class MoveFileCommand : ICommand
{
    private readonly IFileManager _fileManager;
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public MoveFileCommand(IFileManager fileManager, ICurrentDirectoryManager currentDirectoryManager)
    {
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
    }

    public void Execute(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: move <source> <destination>");
            return;
        }

        string sourcePath = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);
        string destinationPath = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[1]);

        try
        {
            _currentDirectoryManager.ValidatePath(sourcePath);
            _currentDirectoryManager.ValidatePath(destinationPath);
            _fileManager.MoveFile(sourcePath, destinationPath);
            Console.WriteLine($"Moved {sourcePath} to {destinationPath}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error moving file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error moving file: {ex.Message}");
        }
    }
}