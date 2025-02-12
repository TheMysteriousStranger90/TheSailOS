using System;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands;

public class CreateDirectoryCommand : ICommand
{
    private readonly IDirectoryManager _directoryManager;
    private readonly ICurrentDirectoryManager _currentDirectoryManager;

    public CreateDirectoryCommand(IDirectoryManager directoryManager, ICurrentDirectoryManager currentDirectoryManager)
    {
        _directoryManager = directoryManager ?? throw new ArgumentNullException(nameof(directoryManager));
        _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
    }

    public void Execute(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: mkdir <path>");
            return;
        }

        string path = _currentDirectoryManager.CombinePath(_currentDirectoryManager.GetCurrentDirectory(), args[0]);

        try
        {
            _currentDirectoryManager.ValidatePath(path);
            _directoryManager.CreateDirectory(path);
            Console.WriteLine($"Directory created: {path}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error creating directory: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating directory: {ex.Message}");
        }
    }
}