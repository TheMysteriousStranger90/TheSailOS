namespace TheSailOSProject.Commands.Helpers;

public interface IAliasManager
{
    void CreateAlias(string alias, string command);
    string GetCommand(string alias);
}