namespace TheSailOSProject.Commands.Helpers;

public class ShowHistoryCommand : ICommand
{
    private readonly ICommandHistoryManager _historyManager;

    public ShowHistoryCommand(ICommandHistoryManager historyManager)
    {
        _historyManager = historyManager;
    }

    public void Execute(string[] args)
    {
        _historyManager.ShowHistory();
    }
}