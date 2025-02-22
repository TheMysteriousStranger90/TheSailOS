using System;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Files;

public class DeleteFileCommand : ICommand
{
    private readonly IFileManager _fileManager;
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public DeleteFileCommand(IFileManager fileManager, ICurrentDirectoryManager currentDirectoryManager)
    {
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
    }

    public void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: delete <path>");
            return;
        }

        string path = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);
        try
        {
            _currentDirectoryManager.ValidatePath(path);
            _fileManager.DeleteFileTheSail(path);
            Console.WriteLine($"File deleted: {path}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
        }
    }
}