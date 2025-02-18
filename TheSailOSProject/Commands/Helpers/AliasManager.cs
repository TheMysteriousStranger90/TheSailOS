using System;
using System.Collections.Generic;
using System.Linq;

namespace TheSailOSProject.Commands.Helpers;

public class AliasManager : IAliasManager
{
    private readonly Dictionary<string, string> _aliases;
    private readonly List<string> _availableCommands;

    public AliasManager(List<string> availableCommands)
    {
        _aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _availableCommands = availableCommands;
    }

    public void CreateAlias(string alias, string command)
    {
        if (string.IsNullOrWhiteSpace(alias))
            throw new ArgumentException("Alias cannot be empty");

        if (string.IsNullOrWhiteSpace(command))
            throw new ArgumentException("Command cannot be empty");

        if (!_availableCommands.Contains(command, StringComparer.OrdinalIgnoreCase) && 
            !_aliases.ContainsKey(command))
        {
            throw new ArgumentException($"Unknown command '{command}'");
        }

        if (_aliases.ContainsKey(command))
        {
            command = ResolveAliasChain(command);
        }

        _aliases[alias.ToLower()] = command.ToLower();
    }

    public string GetCommand(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
            return alias;

        return ResolveAliasChain(alias.ToLower());
    }

    private string ResolveAliasChain(string alias)
    {
        var visited = new HashSet<string>();
        var current = alias;

        while (_aliases.ContainsKey(current) && !visited.Contains(current))
        {
            visited.Add(current);
            current = _aliases[current];
        }

        return current;
    }
}