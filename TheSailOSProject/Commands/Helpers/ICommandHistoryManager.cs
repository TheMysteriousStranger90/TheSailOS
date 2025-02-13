namespace TheSailOSProject.Commands.Helpers;

public interface ICommandHistoryManager
{
    void AddCommand(string command);
    void ShowHistory();
    void Clear();
}