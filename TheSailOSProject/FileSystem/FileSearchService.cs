using System;
using System.Collections.Generic;
using System.IO;

namespace TheSailOSProject.FileSystem
{
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
                Console.WriteLine($"[DEBUG] Searching in directory: {directory}");
                
                try
                {
                    var files = _directoryManager.ListFiles(directory);
                    Console.WriteLine($"[DEBUG] Found {files?.Length ?? 0} files in directory {directory}");

                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            // Get just the filename component for pattern matching
                            string filename = Path.GetFileName(file);
                            if (MatchesPattern(filename, pattern))
                            {
                                results.Add(file);
                                Console.WriteLine($"[DEBUG] Found matching file: {file}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Error listing files in {directory}: {ex.Message}");
                }
                
                if (recursive)
                {
                    try
                    {
                        var subdirectories = _directoryManager.ListDirectories(directory);
                        Console.WriteLine($"[DEBUG] Found {subdirectories?.Length ?? 0} subdirectories in {directory}");

                        if (subdirectories != null && subdirectories.Length > 0)
                        {
                            foreach (var subdir in subdirectories)
                            {
                                Console.WriteLine($"[DEBUG] Raw subdirectory path: '{subdir}'");

                                string fullSubdirPath;

                                if (subdir.Contains(":"))
                                {
                                    fullSubdirPath = subdir;
                                }
                                else if (Path.IsPathRooted(subdir))
                                {
                                    fullSubdirPath = subdir;
                                }
                                else
                                {
                                    if (subdir.Contains("\\"))
                                    {
                                        fullSubdirPath = Path.Combine(directory, subdir);
                                    }
                                    else
                                    {
                                        fullSubdirPath = Path.Combine(directory, subdir);
                                    }
                                }

                                if (!fullSubdirPath.EndsWith("\\"))
                                {
                                    fullSubdirPath += "\\";
                                }

                                Console.WriteLine($"[DEBUG] Recursing into subdirectory: '{fullSubdirPath}'");
                                var subdirResults = FindFiles(pattern, fullSubdirPath, true);
                                results.AddRange(subdirResults);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] Error listing subdirectories in {directory}: {ex.Message}");
                        Console.WriteLine($"[DEBUG] Exception details: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching in {directory}: {ex.Message}");
                Console.WriteLine($"[DEBUG] Exception details: {ex}");
            }

            return results;
        }

        private bool MatchesPattern(string filename, string pattern)
        {
            if (pattern.StartsWith("*") && pattern.EndsWith(".txt"))
            {
                return filename.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
            }

            if (pattern == "*")
                return true;

            if (pattern.StartsWith("*") && pattern.EndsWith("*"))
            {
                var substring = pattern.Substring(1, pattern.Length - 2);
                return filename.Contains(substring, StringComparison.OrdinalIgnoreCase);
            }

            if (pattern.StartsWith("*"))
            {
                var suffix = pattern.Substring(1);
                return filename.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
            }

            if (pattern.EndsWith("*"))
            {
                var prefix = pattern.Substring(0, pattern.Length - 1);
                return filename.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(filename, pattern, StringComparison.OrdinalIgnoreCase);
        }

        private string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path) || path == ".")
                return "0:\\";

            if (path.StartsWith("./") || path.StartsWith(".\\"))
                return "0:\\" + path.Substring(2);

            if (!path.Contains(":"))
                return "0:\\" + path.TrimStart('\\', '/');

            return path.TrimEnd('\\') + "\\";
        }
    }
}