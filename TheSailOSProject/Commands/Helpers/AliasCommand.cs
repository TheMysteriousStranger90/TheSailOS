using System;

namespace TheSailOSProject.Commands.Helpers;

public class AliasCommand : ICommand
{
    private readonly IAliasManager _aliasManager;

    public AliasCommand(IAliasManager aliasManager)
    {
        _aliasManager = aliasManager ?? throw new ArgumentNullException(nameof(aliasManager));
    }

    public void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: alias <new_alias> <command>");
            return;
        }
        
        if (args.Length == 1 && args[0].ToLower() == "-l")
        {
            ShowAllAliases();
            return;
        }

        if (args.Length != 2)
        {
            Console.WriteLine("Usage: alias <new_alias> <command>");
            return;
        }

        string alias = args[0];
        string command = args[1];

        try
        {
            _aliasManager.CreateAlias(alias, command);
            Console.WriteLine($"Alias '{alias}' created for command '{command}'.");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error creating alias: {ex.Message}");
        }
    }

    private void ShowAllAliases()
    {
        Console.WriteLine("Current aliases:");
    }
}