using System;
using System.IO;

namespace TheSailOSProject.Commands.Directories;

public class CurrentDirectoryManager : ICurrentDirectoryManager
{
    private string _currentDirectory;
    private readonly string _rootDirectory;

    public CurrentDirectoryManager(string rootDirectory)
    {
        _rootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
        _currentDirectory = _rootDirectory;
    }

    public string GetCurrentDirectory()
    {
        return _currentDirectory;
    }

    public void SetCurrentDirectory(string path)
    {
        _currentDirectory = path;
    }

    public string CombinePath(string basePath, string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
        {
            return basePath;
        }

        if (Path.IsPathRooted(relativePath))
        {
            return relativePath;
        }

        char lastChar = basePath[basePath.Length - 1];
        if (lastChar != '\\')
        {
            return basePath + "\\" + relativePath;
        }
        else
        {
            return basePath + relativePath;
        }
    }

    public void ValidatePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path cannot be null or empty.");
        }

        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            throw new ArgumentException("Path contains invalid characters.");
        }

        if (!path.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Path is outside the allowed file system root.");
        }
    }
}