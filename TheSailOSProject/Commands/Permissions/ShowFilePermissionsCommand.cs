using System;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.Permissions;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Permissions;

public class ShowFilePermissionsCommand : ICommand
{
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public ShowFilePermissionsCommand(ICurrentDirectoryManager currentDirectoryManager)
    {
        _currentDirectoryManager = currentDirectoryManager;
    }

    public void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: permissions <path>");
            return;
        }

        string path = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);
            
        try
        {
            _currentDirectoryManager.ValidatePath(path);
            var permissions = PermissionsManager.GetFilePermissions(path);
                
            if (permissions != null)
            {
                ConsoleManager.WriteLineColored($"File: {path}", ConsoleStyle.Colors.Primary);
                ConsoleManager.WriteLineColored($"Owner: {permissions.OwnerUsername}", ConsoleStyle.Colors.Accent);
                ConsoleManager.WriteLineColored($"Read Permission: {permissions.AllowRead}", 
                    permissions.AllowRead ? ConsoleStyle.Colors.Success : ConsoleStyle.Colors.Error);
                ConsoleManager.WriteLineColored($"Write Permission: {permissions.AllowWrite}", 
                    permissions.AllowWrite ? ConsoleStyle.Colors.Success : ConsoleStyle.Colors.Error);
            }
            else
            {
                Console.WriteLine("No permissions found for this file.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting permissions: {ex.Message}");
        }
    }
}