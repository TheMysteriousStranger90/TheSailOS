using System;
using TheSailOSProject.Applications;
using TheSailOSProject.FileSystem;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Processes.Applications;

public class TextEditorProcess : Process
{
    private readonly string _filePath;
    private readonly IFileManager _fileManager;

    public TextEditorProcess(string filePath, IFileManager fileManager)
        : base("TextEditor", ProcessType.Application)
    {
        _filePath = filePath;
        _fileManager = fileManager;
    }

    public override void Start()
    {
        base.Start();
        Console.Clear();
        Console.WriteLine("Starting Text Editor...");
        Console.WriteLine("Controls:");
        Console.WriteLine("  - Type text commands to edit the file");
        Console.WriteLine("  - 'save' to save, 'list' to show file, 'exit' to quit");
        Console.WriteLine("\nPress any key to start...");
        Console.ReadKey(true);
    }

    public override void Run()
    {
        TextEditor.Run(_filePath, _fileManager);
        Stop();
    }

    public override void Stop()
    {
        base.Stop();
        Console.Clear();
        ConsoleManager.WriteLineColored("[TextEditor] Closed", ConsoleStyle.Colors.Primary);
    }
}