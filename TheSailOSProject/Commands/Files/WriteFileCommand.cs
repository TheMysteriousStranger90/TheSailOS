using System;
using System.Linq;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Files;

public class WriteFileCommand : ICommand
{
    private readonly IFileManager _fileManager;
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public WriteFileCommand(IFileManager fileManager, ICurrentDirectoryManager currentDirectoryManager)
    {
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        _currentDirectoryManager =
            currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
    }

    public void Execute(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: write <path> <content>");
            return;
        }

        string path = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);
        string content = string.Join(" ", args.Skip(1));
        try
        {
            _currentDirectoryManager.ValidatePath(path);
            _fileManager.WriteFile(path, content);
            Console.WriteLine($"Wrote to file: {path}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error writing to file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to file: {ex.Message}");
        }
    }
}