using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.System.FileSystem.VFS;
using TheSailOS.Commands;
using TheSailOS.FileSystem;
using Sys = Cosmos.System;

namespace TheSailOS
{
    public class Kernel : Sys.Kernel
    {
        public static string CurrentDirectory { get; private set; } = @"L:\";
        public FileTheSail _fileTheSail;
        public static FileTheSail CurrentFileTheSail;
        
        private CommandProcessor _commandProcessor;

        public static void SetCurrentDirectory(string path)
        {
            CurrentDirectory = path;
        }

        protected override void BeforeRun()
        {
            _fileTheSail = new FileTheSail();
            VFSManager.RegisterVFS(_fileTheSail._vfs);
            CurrentFileTheSail = _fileTheSail;
            
            var fileReader = new FileReader(_fileTheSail);
            var fileWriter = new FileWriter(_fileTheSail);
            var fileMover = new FileMover(_fileTheSail, fileReader, fileWriter);
            var fileSystemOperations = new FileSystemOperations(_fileTheSail);

            _commandProcessor = new CommandProcessor(fileReader, fileWriter, fileMover, fileSystemOperations);

            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
            Console.WriteLine("Type a command to execute.");
        }

        protected override void Run()
        {
            Console.Write($"{CurrentDirectory}> ");
            var input = Console.ReadLine();
            
            _commandProcessor.ProcessCommand(input);
        }
    }
}