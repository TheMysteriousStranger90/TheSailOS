using System;
using System.IO;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Directories;

public class BackCommand : ICommand
{
    private readonly ICurrentDirectoryManager _currentDirectoryManager;
    private readonly IRootDirectoryProvider _rootDirectoryProvider;

    public BackCommand(ICurrentDirectoryManager currentDirectoryManager, IRootDirectoryProvider rootDirectoryProvider)
    {
        _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
        _rootDirectoryProvider = rootDirectoryProvider ?? throw new ArgumentNullException(nameof(rootDirectoryProvider));
    }

    public void Execute(string[] args)
    {
        if (args.Length > 0)
        {
            Console.WriteLine("Usage: back");
            return;
        }

        string currentDirectory = _currentDirectoryManager.GetCurrentDirectory();
        string rootDirectory = _rootDirectoryProvider.GetRootDirectory();

        if (currentDirectory != rootDirectory)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(currentDirectory);
            if (dirInfo.Parent != null)
            {
                _currentDirectoryManager.SetCurrentDirectory(dirInfo.Parent.FullName);
            }
            else
            {
                _currentDirectoryManager.SetCurrentDirectory(rootDirectory);
            }
        }
        Console.WriteLine($"Current directory: {_currentDirectoryManager.GetCurrentDirectory()}");
    }
}