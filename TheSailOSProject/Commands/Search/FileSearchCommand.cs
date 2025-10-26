using System;
using TheSailOSProject.FileSystem;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Search;

public class FileSearchCommand : ICommand
{
    private readonly FileSearchService _searchService;
    public string Name => "find";
    public string Description => "Search for files matching a pattern";

    public FileSearchCommand(FileSearchService searchService)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
    }

    public void Execute(string[] args)
    {
        if (args.Length < 1)
        {
            ShowUsage();
            return;
        }

        string pattern = args[0];
        string path = "0:\\";
        bool recursive = false;

        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "-r" || args[i] == "-recursive")
            {
                recursive = true;
            }
            else if (!args[i].StartsWith("-"))
            {
                path = args[i];
            }
        }

        ExecuteSearch(pattern, path, recursive);
    }

    private void ExecuteSearch(string pattern, string path, bool recursive)
    {
        try
        {
            ConsoleManager.WriteColored("Searching for: ", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteColored($"'{pattern}'", ConsoleStyle.Colors.Accent);
            ConsoleManager.WriteColored(" in ", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored(path, ConsoleStyle.Colors.FilePath);

            if (recursive)
            {
                ConsoleManager.WriteLineColored("Recursive search enabled", ConsoleStyle.Colors.Warning);
            }

            Console.WriteLine();

            var startTime = System.DateTime.Now;
            var files = _searchService.FindFiles(pattern, path, recursive);
            var endTime = System.DateTime.Now;
            var duration = (endTime - startTime).TotalMilliseconds;

            if (files.Count == 0)
            {
                ConsoleManager.WriteLineColored($"No files matching '{pattern}' found",
                    ConsoleStyle.Colors.Warning);
            }
            else
            {
                ConsoleManager.WriteColored($"Found ", ConsoleStyle.Colors.Success);
                ConsoleManager.WriteColored($"{files.Count}", ConsoleStyle.Colors.Number);
                ConsoleManager.WriteLineColored($" file(s):", ConsoleStyle.Colors.Success);
                Console.WriteLine(new string('=', 70));

                foreach (var file in files)
                {
                    DisplayFileResult(file);
                }

                Console.WriteLine(new string('=', 70));
            }

            ConsoleManager.WriteColored("Search completed in ", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteColored($"{duration:F2}", ConsoleStyle.Colors.Number);
            ConsoleManager.WriteLineColored(" ms", ConsoleStyle.Colors.Primary);
        }
        catch (Exception ex)
        {
            ConsoleManager.WriteLineColored($"Error searching for files: {ex.Message}",
                ConsoleStyle.Colors.Error);
        }
    }

    private void DisplayFileResult(string filePath)
    {
        try
        {
            var fileInfo = new System.IO.FileInfo(filePath);

            ConsoleManager.WriteColored("  ", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteColored(fileInfo.Name, ConsoleStyle.Colors.Accent);

            ConsoleManager.WriteColored(" (", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteColored(FormatFileSize(fileInfo.Length), ConsoleStyle.Colors.Number);
            ConsoleManager.WriteColored(")", ConsoleStyle.Colors.Primary);

            Console.WriteLine();

            ConsoleManager.WriteColored("    ", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored(filePath, ConsoleStyle.Colors.FilePath);
        }
        catch
        {
            ConsoleManager.WriteColored("  ", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteLineColored(filePath, ConsoleStyle.Colors.FilePath);
        }
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    private void ShowUsage()
    {
        ConsoleManager.WriteLineColored("File Search Command", ConsoleStyle.Colors.Primary);
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  find <pattern>              - Search in current directory (0:\\)");
        Console.WriteLine("  find <pattern> <path>       - Search in specified directory");
        Console.WriteLine("  find <pattern> -r           - Search recursively in current directory");
        Console.WriteLine("  find <pattern> <path> -r    - Search recursively in specified directory");
        Console.WriteLine();
        Console.WriteLine("Pattern wildcards:");
        Console.WriteLine("  *           - Match any characters");
        Console.WriteLine("  *.txt       - Match all .txt files");
        Console.WriteLine("  test*       - Match files starting with 'test'");
        Console.WriteLine("  *report*    - Match files containing 'report'");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  find *.txt                  - Find all .txt files in root");
        Console.WriteLine("  find *.cs 0:\\System         - Find all .cs files in System folder");
        Console.WriteLine("  find config* 0:\\ -r         - Find all files starting with 'config' recursively");
        Console.WriteLine("  find *log* 0:\\Logs -r       - Find all files containing 'log' in Logs folder");
    }
}