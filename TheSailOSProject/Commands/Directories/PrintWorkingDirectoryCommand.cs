using System;

namespace TheSailOSProject.Commands.Directories;

public class PrintWorkingDirectoryCommand : ICommand
{
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public PrintWorkingDirectoryCommand(ICurrentDirectoryManager currentDirectoryManager)
    {
        _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
    }

    public void Execute(string[] args)
    {
        Console.WriteLine(_currentDirectoryManager.GetCurrentDirectory());
    }
}