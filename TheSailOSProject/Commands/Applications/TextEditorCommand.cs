using System;
using System.Threading;
using TheSailOSProject.FileSystem;
using TheSailOSProject.Processes;
using TheSailOSProject.Processes.Applications;

namespace TheSailOSProject.Commands.Applications;

public class TextEditorCommand : ICommand
{
    private readonly IFileManager _fileManager;

    public TextEditorCommand(IFileManager fileManager)
    {
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
    }

    public void Execute(string[] args)
    {
        string filePath = args.Length > 0 ? args[0] : null;

        var editorProcess = new TextEditorProcess(filePath, _fileManager);
        ProcessManager.Register(editorProcess);
        ProcessManager.Start(editorProcess);

        while (editorProcess.IsRunning)
        {
            ProcessManager.Update();
            Thread.Sleep(100);
        }
    }

    public string HelpText()
    {
        return "Opens the text editor application.\nUsage: textedit <path>";
    }
}