using System;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Files;

public class ReadFileCommand : ICommand
{
    private readonly IFileManager _fileManager;
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public ReadFileCommand(IFileManager fileManager, ICurrentDirectoryManager currentDirectoryManager)
    {
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
    }

    public void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: read <path>");
            return;
        }

        string path = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);
        try
        {
            _currentDirectoryManager.ValidatePath(path);
            string content = _fileManager.ReadFile(path);
            if (content != null)
            {
                Console.WriteLine($"Content of {path}:\n{content}");
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
        }
    }
}