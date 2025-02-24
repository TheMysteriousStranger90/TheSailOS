using System;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.FileSystem;
using TheSailOSProject.Permissions;

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
            
            var currentUser = Kernel.CurrentUser;
            
            if (!PermissionsManager.CanReadFile(path, currentUser))
            {
                Console.WriteLine("Access denied: You don't have permission to read this file.");
                return;
            }

            string content = _fileManager.ReadFile(path);
            if (content != null)
            {
                Console.WriteLine($"Content of {path}:");
                string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (string line in lines)
                {
                    Console.WriteLine(line);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
        }
    }
}