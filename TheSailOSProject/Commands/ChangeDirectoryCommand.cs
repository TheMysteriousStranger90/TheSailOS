using System;
using System.IO;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands;

public class ChangeDirectoryCommand : ICommand
{
    private readonly IDirectoryManager _directoryManager;
    private readonly ICurrentDirectoryManager _currentDirectoryManager;
    private readonly IRootDirectoryProvider _rootDirectoryProvider;

    public ChangeDirectoryCommand(IDirectoryManager directoryManager, ICurrentDirectoryManager currentDirectoryManager, IRootDirectoryProvider rootDirectoryProvider)
    {
        _directoryManager = directoryManager ?? throw new ArgumentNullException(nameof(directoryManager));
        _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
        _rootDirectoryProvider = rootDirectoryProvider ?? throw new ArgumentNullException(nameof(rootDirectoryProvider));
    }

    public void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: cd <path>");
            return;
        }

        string path = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);

        try
        {
            _currentDirectoryManager.ValidatePath(path);

            if (args[0] == "..")
            {
                HandleParentDirectory();
                return;
            }

            if (Directory.Exists(path))
            {
                _currentDirectoryManager.SetCurrentDirectory(path);
                Console.WriteLine($"Current directory: {_currentDirectoryManager.GetCurrentDirectory()}");
            }
            else
            {
                Console.WriteLine($"Directory not found: {path}");
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error changing directory: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error changing directory: {ex.Message}");
        }
    }

    private void HandleParentDirectory()
    {
        string currentDirectory = _currentDirectoryManager.GetCurrentDirectory();
        if (currentDirectory != _rootDirectoryProvider.GetRootDirectory())
        {
            DirectoryInfo dirInfo = new DirectoryInfo(currentDirectory);
            if (dirInfo.Parent != null)
            {
                _currentDirectoryManager.SetCurrentDirectory(dirInfo.Parent.FullName);
            }
            else
            {
                _currentDirectoryManager.SetCurrentDirectory(_rootDirectoryProvider.GetRootDirectory());
            }
        }
        Console.WriteLine($"Current directory: {_currentDirectoryManager.GetCurrentDirectory()}");
    }
}