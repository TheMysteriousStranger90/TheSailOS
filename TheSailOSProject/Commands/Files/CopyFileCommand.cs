using System;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Files;

public class CopyFileCommand : ICommand
{
    private readonly IFileManager _fileManager;
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public CopyFileCommand(IFileManager fileManager, ICurrentDirectoryManager currentDirectoryManager)
    {
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
    }

    public void Execute(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: copy <source> <destination>");
            return;
        }

        string source = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);
        string destination = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[1]);
        try
        {
            _currentDirectoryManager.ValidatePath(source);
            _currentDirectoryManager.ValidatePath(destination);
            _fileManager.CopyFile(source, destination);
            Console.WriteLine($"Copied {source} to {destination}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error copying file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copying file: {ex.Message}");
        }
    }
}