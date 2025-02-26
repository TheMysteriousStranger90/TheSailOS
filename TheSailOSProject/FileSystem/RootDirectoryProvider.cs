using System;
using System.IO;

namespace TheSailOSProject.FileSystem;

public class RootDirectoryProvider : IRootDirectoryProvider
{
    private readonly string _rootDirectory;

    public RootDirectoryProvider(string rootDirectory = null)
    {
        string dir = rootDirectory ?? "0:\\";

        if (!dir.EndsWith("\\"))
        {
            dir += "\\";
        }
        
        try
        {
            if (!Directory.Exists(dir))
            {
                Console.WriteLine($"[INFO] Root directory does not exist: {dir}, trying to create");
                Directory.CreateDirectory(dir);
                Console.WriteLine($"[INFO] Created root directory: {dir}");
            }
            _rootDirectory = dir;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Could not create root directory: {ex.Message}");
            _rootDirectory = "0:\\";
        }
        
        Console.WriteLine($"[INFO] Using root directory: {_rootDirectory}");
    }

    public string GetRootDirectory()
    {
        return _rootDirectory;
    }
}