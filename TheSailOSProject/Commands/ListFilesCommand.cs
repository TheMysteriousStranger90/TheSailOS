using System;
using System.IO;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands;

public class ListFilesCommand : ICommand
{
    private readonly IFileManager _fileManager;
    private readonly IDirectoryManager _directoryManager;
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public ListFilesCommand(IFileManager fileManager, IDirectoryManager directoryManager, ICurrentDirectoryManager currentDirectoryManager)
    {
        _fileManager = fileManager;
        _directoryManager = directoryManager;
        _currentDirectoryManager = currentDirectoryManager;
    }

    public void Execute(string[] args)
    {
        string path = args.Length > 0 ? _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]) : _currentDirectoryManager.GetCurrentDirectory();
        try
        {
            _currentDirectoryManager.ValidatePath(path);
            string[] files = _directoryManager.ListFiles(path);
            if (files != null)
            {
                foreach (string file in files)
                {
                    Console.WriteLine(Path.GetFileName(file));
                }
            }

            string[] directories = _directoryManager.ListDirectories(path);
            if (directories != null)
            {
                foreach (string dir in directories)
                {
                    Console.WriteLine(Path.GetFileName(dir));
                }
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error listing files: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing files: {ex.Message}");
        }
    }
}