using System;
using System.IO;
using System.Linq;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.FileSystem;
using TheSailOSProject.Permissions;
using TheSailOSProject.Security;
using TheSailOSProject.Users;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Files
{
    public class WriteFileCommand : ICommand
    {
        private readonly IFileManager _fileManager;
        private readonly ICurrentDirectoryManager _currentDirectoryManager;

        public WriteFileCommand(IFileManager fileManager, ICurrentDirectoryManager currentDirectoryManager)
        {
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
        }

        public void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: write <path> <content>");
                return;
            }
            
            var currentUser = Kernel.CurrentUser;
            if (currentUser == null)
            {
                ConsoleManager.WriteLineColored("Error: No user logged in", ConsoleStyle.Colors.Error);
                return;
            }

            string path = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);
            string content = string.Join(" ", args.Skip(1));

            try
            {
                _currentDirectoryManager.ValidatePath(path);
                
                var permissions = PermissionsManager.GetFilePermissions(path);
                
                if (permissions != null)
                {
                    if (!CanWriteFile(currentUser, permissions))
                    {
                        ConsoleManager.WriteLineColored("Access denied: You don't have permission to write to this file", ConsoleStyle.Colors.Error);
                        return;
                    }
                }
                else
                {
                    if (!File.Exists(path))
                    {
                        PermissionsManager.SetFilePermissions(path, currentUser.Username, true, true);
                    }
                    else
                    {
                        PermissionsManager.SetFilePermissions(path, currentUser.Username, true, true);
                    }
                }

                _fileManager.WriteFile(path, content);
                ConsoleManager.WriteLineColored($"Wrote to file: {path}", ConsoleStyle.Colors.Success);
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error writing to file: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }

        private bool CanWriteFile(User user, FilePermissions permissions)
        {
            return user.Type == UserType.Administrator || 
                   (permissions.OwnerUsername == user.Username && permissions.AllowWrite);
        }
    }
}