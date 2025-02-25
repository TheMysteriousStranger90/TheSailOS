using System;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.FileSystem;
using TheSailOSProject.Permissions;

namespace TheSailOSProject.Commands.Files;

public class CreateFileCommand : ICommand
{
    private readonly IFileManager _fileManager;
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public CreateFileCommand(IFileManager fileManager, ICurrentDirectoryManager currentDirectoryManager)
    {
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
    }

    public void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: create <path>");
            return;
        }

        string path = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);
        try
        {
            _currentDirectoryManager.ValidatePath(path);
            
            var currentUser = Kernel.CurrentUser;
            if (currentUser == null)
            {
                Console.WriteLine("Error: No user logged in");
                return;
            }

            _fileManager.CreateFileTheSail(path);
            
            PermissionsManager.SetFilePermissions(path, currentUser.Username, true, true);
            
            Console.WriteLine($"File created: {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating file: {ex.Message}");
        }
    }
}