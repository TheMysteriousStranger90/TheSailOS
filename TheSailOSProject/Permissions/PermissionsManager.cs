using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheSailOSProject.Users;

namespace TheSailOSProject.Permissions;

public static class PermissionsManager
{
    private static readonly Dictionary<string, FilePermissions> _filePermissions =
        new Dictionary<string, FilePermissions>();

    private const string PermissionsFile = @"0:\System\permissions.dat";

    public static void Initialize()
    {
        if (File.Exists(PermissionsFile))
        {
            LoadPermissions();
        }
    }

    public static bool CanReadFile(string filePath, User user)
    {
        if (user.Type == UserType.Administrator)
            return true;

        return _filePermissions.ContainsKey(filePath) &&
               (_filePermissions[filePath].OwnerUsername == user.Username &&
                _filePermissions[filePath].AllowRead);
    }

    public static bool CanWriteFile(string filePath, User user)
    {
        if (user.Type == UserType.Administrator)
            return true;

        return _filePermissions.ContainsKey(filePath) &&
               (_filePermissions[filePath].OwnerUsername == user.Username &&
                _filePermissions[filePath].AllowWrite);
    }

    public static void SetFilePermissions(string filePath, string ownerUsername, bool allowRead = true,
        bool allowWrite = true)
    {
        _filePermissions[filePath] = new FilePermissions(filePath, ownerUsername, allowRead, allowWrite);
        SavePermissions();
    }

    private static void LoadPermissions()
    {
        var lines = File.ReadAllLines(PermissionsFile);
        foreach (var line in lines)
        {
            var parts = line.Split('|');
            if (parts.Length == 4)
            {
                _filePermissions[parts[0]] = new FilePermissions(
                    parts[0],
                    parts[1],
                    bool.Parse(parts[2]),
                    bool.Parse(parts[3])
                );
            }
        }
    }

    private static void SavePermissions()
    {
        var lines = _filePermissions.Values.Select(p =>
            $"{p.FilePath}|{p.OwnerUsername}|{p.AllowRead}|{p.AllowWrite}");
        File.WriteAllLines(PermissionsFile, lines);
    }
    
    public static FilePermissions GetFilePermissions(string filePath)
    {
        return _filePermissions.ContainsKey(filePath) ? _filePermissions[filePath] : null;
    }
}