using System;
using System.Collections.Generic;

namespace TheSailOSProject.Commands;

public class HelpCommand : ICommand
{
    private readonly Dictionary<string, string> _helpTexts;

    public HelpCommand(Dictionary<string, string> helpTexts)
    {
        _helpTexts = helpTexts;
    }

    public void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Available commands:");
            foreach (var command in _helpTexts.Keys)
            {
                Console.WriteLine($"- {command}");
            }
            Console.WriteLine("Type 'help <command>' for more information.");
        }
        else
        {
            string command = args[0];
            if (_helpTexts.ContainsKey(command))
            {
                Console.WriteLine(_helpTexts[command]);
            }
            else
            {
                Console.WriteLine($"No help available for command: {command}");
            }
        }
    }
}