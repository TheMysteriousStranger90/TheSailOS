using System;

namespace TheSailOSProject.Commands;

public class ClearHistoryCommand : ICommand
{
    private readonly ICommandHistoryManager _historyManager;

    public ClearHistoryCommand(ICommandHistoryManager historyManager)
    {
        _historyManager = historyManager;
    }

    public void Execute(string[] args)
    {
        _historyManager.Clear();
        Console.WriteLine("Command history cleared.");
    }
}