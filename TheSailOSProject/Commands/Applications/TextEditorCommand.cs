using TheSailOSProject.Applications;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Applications
{
    public class TextEditorCommand : ICommand
    {
        public void Execute(string[] args)
        {
            string filePath = args.Length > 0 ? args[0] : null;
            TextEditor.Run(filePath);
        }

        public string HelpText()
        {
            return "Opens the text editor application.\nUsage: edit <path>";
        }
    }
}