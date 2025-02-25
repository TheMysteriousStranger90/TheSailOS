using System;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.Permissions;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Permissions;

public class SetPermissionsCommand : ICommand
{
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public SetPermissionsCommand(ICurrentDirectoryManager currentDirectoryManager)
    {
        _currentDirectoryManager = currentDirectoryManager;
    }

    public void Execute(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Usage: setpermissions <path> <username> <allowRead> <allowWrite>");
            Console.WriteLine("Example: setpermissions myfile.txt user1 true false");
            return;
        }

        var currentUser = Kernel.CurrentUser;
        if (currentUser == null)
        {
            ConsoleManager.WriteLineColored("Error: No user logged in", ConsoleStyle.Colors.Error);
            return;
        }

        if (currentUser.Type != UserType.Administrator)
        {
            ConsoleManager.WriteLineColored("Access denied: Only administrators can set file permissions",
                ConsoleStyle.Colors.Error);
            return;
        }

        string path = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);
        string targetUsername = args[1];

        if (!bool.TryParse(args[2], out bool allowRead) || !bool.TryParse(args[3], out bool allowWrite))
        {
            ConsoleManager.WriteLineColored("Error: Allow read and allow write must be 'true' or 'false'",
                ConsoleStyle.Colors.Error);
            return;
        }

        try
        {
            _currentDirectoryManager.ValidatePath(path);

            var targetUser = UserManager.GetUser(targetUsername);
            if (targetUser == null)
            {
                ConsoleManager.WriteLineColored($"Error: User '{targetUsername}' not found", ConsoleStyle.Colors.Error);
                return;
            }

            PermissionsManager.SetFilePermissions(path, targetUsername, allowRead, allowWrite);

            ConsoleManager.WriteLineColored("File permissions updated successfully:", ConsoleStyle.Colors.Success);
            ConsoleManager.WriteLineColored($"File: {path}", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored($"Owner: {targetUsername}", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored($"Read Permission: {allowRead}",
                allowRead ? ConsoleStyle.Colors.Success : ConsoleStyle.Colors.Warning);
            ConsoleManager.WriteLineColored($"Write Permission: {allowWrite}",
                allowWrite ? ConsoleStyle.Colors.Success : ConsoleStyle.Colors.Warning);
        }
        catch (Exception ex)
        {
            ConsoleManager.WriteLineColored($"Error setting permissions: {ex.Message}", ConsoleStyle.Colors.Error);
        }
    }
}