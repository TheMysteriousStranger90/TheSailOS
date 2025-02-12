namespace TheSailOSProject.Commands;

public interface IAliasManager
{
    void CreateAlias(string alias, string command);
    string GetCommand(string alias);
}