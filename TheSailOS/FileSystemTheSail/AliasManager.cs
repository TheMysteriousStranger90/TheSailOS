using System;
using System.Collections.Generic;

namespace TheSailOS.FileSystemTheSail;

public class AliasManager
{
    private Dictionary<string, string> _aliases;
    private List<string> _availableCommands;

    public AliasManager(List<string> availableCommands)
    {
        _aliases = new Dictionary<string, string>();
        _availableCommands = availableCommands;
    }

    public void CreateAlias(string alias, string command)
    {
        if (!_availableCommands.Contains(command))
            throw new ArgumentException($"Unknown command '{command}'");

        _aliases[alias] = command;
    }

    public string GetCommand(string alias)
    {
        return _aliases.ContainsKey(alias) ? _aliases[alias] : alias;
    }
}