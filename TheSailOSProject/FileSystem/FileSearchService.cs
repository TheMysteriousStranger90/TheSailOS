using System;
using System.Collections.Generic;
using System.IO;

namespace TheSailOSProject.FileSystem;

public class FileSearchService
{
    private readonly IFileManager _fileManager;
    private readonly IDirectoryManager _directoryManager;

    public FileSearchService(IFileManager fileManager, IDirectoryManager directoryManager)
    {
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        _directoryManager = directoryManager ?? throw new ArgumentNullException(nameof(directoryManager));
    }

    public List<string> FindFiles(string pattern, string directory, bool recursive)
    {
        var results = new List<string>();

        try
        {
            directory = NormalizePath(directory);

            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Directory not found: {directory}");
                return results;
            }

            SearchInDirectory(pattern, directory, results);

            if (recursive)
            {
                SearchSubdirectoriesRecursive(pattern, directory, results);
            }
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Access denied to directory: {directory}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching in {directory}: {ex.Message}");
        }

        return results;
    }

    private void SearchInDirectory(string pattern, string directory, List<string> results)
    {
        try
        {
            var files = _directoryManager.ListFiles(directory);

            if (files != null && files.Length > 0)
            {
                foreach (var file in files)
                {
                    string filename = Path.GetFileName(file);

                    if (MatchesPattern(filename, pattern))
                    {
                        results.Add(file);
                    }
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing files in {directory}: {ex.Message}");
        }
    }

    private void SearchSubdirectoriesRecursive(string pattern, string directory, List<string> results)
    {
        try
        {
            var subdirectories = _directoryManager.ListDirectories(directory);

            if (subdirectories != null && subdirectories.Length > 0)
            {
                foreach (var subdir in subdirectories)
                {
                    try
                    {
                        string fullSubdirPath = NormalizeSubdirectoryPath(subdir, directory);

                        SearchInDirectory(pattern, fullSubdirPath, results);

                        SearchSubdirectoriesRecursive(pattern, fullSubdirPath, results);
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error accessing subdirectory: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing subdirectories in {directory}: {ex.Message}");
        }
    }

    private string NormalizeSubdirectoryPath(string subdir, string parentDir)
    {
        string fullPath;

        if (subdir.Contains(":"))
        {
            fullPath = subdir;
        }
        else if (Path.IsPathRooted(subdir))
        {
            fullPath = subdir;
        }
        else
        {
            fullPath = Path.Combine(parentDir, subdir);
        }

        if (!fullPath.EndsWith("\\"))
        {
            fullPath += "\\";
        }

        return fullPath;
    }

    private bool MatchesPattern(string filename, string pattern)
    {
        if (pattern == "*" || pattern == "*.*")
            return true;

        if (!pattern.Contains("*"))
            return string.Equals(filename, pattern, StringComparison.OrdinalIgnoreCase);

        if (pattern.StartsWith("*") && !pattern.Substring(1).Contains("*"))
        {
            string extension = pattern.Substring(1);
            return filename.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.EndsWith("*") && !pattern.Substring(0, pattern.Length - 1).Contains("*"))
        {
            string namePrefix = pattern.Substring(0, pattern.Length - 1);
            string filenameWithoutExt = Path.GetFileNameWithoutExtension(filename);
            return string.Equals(filenameWithoutExt, namePrefix, StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.StartsWith("*") && pattern.EndsWith("*") && pattern.Length > 2)
        {
            string substring = pattern.Substring(1, pattern.Length - 2);
            return filename.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        if (pattern.EndsWith("*") && !pattern.StartsWith("*"))
        {
            string prefix = pattern.Substring(0, pattern.Length - 1);
            return filename.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.StartsWith("*") && !pattern.EndsWith("*"))
        {
            string suffix = pattern.Substring(1);
            return filename.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }

        return MatchesComplexPattern(filename, pattern);
    }

    private bool MatchesComplexPattern(string filename, string pattern)
    {
        string[] parts = pattern.Split('*');
        int currentPos = 0;

        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];

            if (string.IsNullOrEmpty(part))
                continue;

            int foundPos = filename.IndexOf(part, currentPos, StringComparison.OrdinalIgnoreCase);

            if (foundPos == -1)
                return false;

            if (i == 0 && !pattern.StartsWith("*") && foundPos != 0)
                return false;

            if (i == parts.Length - 1 && !pattern.EndsWith("*"))
            {
                if (foundPos + part.Length != filename.Length)
                    return false;
            }

            currentPos = foundPos + part.Length;
        }

        return true;
    }

    private string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path) || path == ".")
            return "0:\\";

        if (path.StartsWith("./") || path.StartsWith(".\\"))
            return "0:\\" + path.Substring(2);

        if (!path.Contains(":"))
        {
            path = "0:\\" + path.TrimStart('\\', '/');
        }

        if (!path.EndsWith("\\"))
            path += "\\";

        return path;
    }
}