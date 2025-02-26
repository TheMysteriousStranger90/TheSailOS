using System;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Search;

public class FindCommand : ICommand
{
    private readonly FileSearchService _searchService;

    public FindCommand(FileSearchService searchService)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
    }

    public void Execute(string[] args)
    {
        var fileSearchCommands = new FileSearchCommands(_searchService);
        fileSearchCommands.ExecuteFindCommand(args);
    }
}