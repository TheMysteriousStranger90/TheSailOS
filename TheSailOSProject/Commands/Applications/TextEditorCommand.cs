using System;
using TheSailOSProject.Applications;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Applications
{
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
            TextEditor.Run(filePath, _fileManager);
        }

        public string HelpText()
        {
            return "Opens the text editor application.\nUsage: edit <path>";
        }
    }
}