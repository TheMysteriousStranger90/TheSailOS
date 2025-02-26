using System;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Search
{
    public class FileSearchCommands
    {
        private readonly FileSearchService _searchService;

        public FileSearchCommands(FileSearchService searchService)
        {
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        }

        public void ExecuteFindCommand(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: find <pattern> [path] [-r]");
                Console.WriteLine("  -r: Search recursively");
                return;
            }

            var pattern = args[0];
            var path = args.Length > 1 && !args[1].StartsWith("-") ? args[1] : ".";
            
            var recursive = true;
            if (args.Length > 1 && Array.IndexOf(args, "-nr") >= 0)
            {
                recursive = false;
            }

            Console.WriteLine($"[DEBUG] Starting search with pattern: {pattern}, path: {path}, recursive: {recursive}");

            try
            {
                var files = _searchService.FindFiles(pattern, path, recursive);
                
                if (files.Count == 0)
                {
                    Console.WriteLine($"No files matching '{pattern}' found in {path}");
                    return;
                }

                Console.WriteLine($"Found {files.Count} file(s) matching '{pattern}':");
                foreach (var file in files)
                {
                    Console.WriteLine($"  {file}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching for files: {ex.Message}");
            }
        }
    }
}