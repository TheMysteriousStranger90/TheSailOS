namespace TheSailOSProject.Commands;

public interface ICommandHistoryManager
{
    void AddCommand(string command);
    void ShowHistory();
    void Clear();
}