using System;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.FileSystem;
using TheSailOSProject.Permissions;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Files;

public class DeleteFileCommand : ICommand
{
    private readonly IFileManager _fileManager;
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public DeleteFileCommand(IFileManager fileManager, ICurrentDirectoryManager currentDirectoryManager)
    {
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        _currentDirectoryManager =
            currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
    }

    public void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: delete <path>");
            return;
        }
        
        var currentUser = Kernel.CurrentUser;
        if (currentUser == null)
        {
            Console.WriteLine("Error: No user logged in");
            return;
        }

        string path = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);
        try
        {
            _currentDirectoryManager.ValidatePath(path);

            // Check if user has permission to delete the file
            var permissions = PermissionsManager.GetFilePermissions(path);
            if (permissions == null)
            {
                Console.WriteLine("Error: File permissions not found");
                return;
            }

            // Allow deletion if user is admin or file owner
            if (currentUser.Type == UserType.Administrator ||
                permissions.OwnerUsername == currentUser.Username)
            {
                _fileManager.DeleteFileTheSail(path);

                PermissionsManager.RemoveFilePermissions(path);

                Console.WriteLine($"File deleted: {path}");
            }
            else
            {
                Console.WriteLine("Access denied: You don't have permission to delete this file");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
        }
    }
}