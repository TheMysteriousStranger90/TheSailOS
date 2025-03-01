﻿using System;
using System.Collections.Generic;

namespace TheSailOSProject.Commands.Helpers;

public class CommandHistoryManager : ICommandHistoryManager
{
    private List<string> _commandHistory;

    public CommandHistoryManager()
    {
        _commandHistory = new List<string>();
    }

    public void AddCommand(string command)
    {
        _commandHistory.Add(command);
    }

    public void ShowHistory()
    {
        for (int i = 0; i < _commandHistory.Count; i++)
        {
            Console.WriteLine($"{i + 1}: {_commandHistory[i]}");
        }
    }

    public void Clear()
    {
        _commandHistory.Clear();
    }
}