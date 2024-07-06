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
        public static string CurrentDirectory { get; private set; } = @"0:\";
        public static FileTheSail CurrentFileTheSail;

        private CommandProcessor _commandProcessor;

        public static void SetCurrentDirectory(string path)
        {
            CurrentDirectory = path;
        }

        protected override void BeforeRun()
        {
            CurrentFileTheSail = new FileTheSail();
            VFSManager.RegisterVFS(CurrentFileTheSail._vfs);
            
            var fileReader = new FileReader(CurrentFileTheSail);
            var fileWriter = new FileWriter(CurrentFileTheSail);
            var fileMover = new FileMover(CurrentFileTheSail, fileReader, fileWriter);
            var fileSystemOperations = new FileSystemOperations(CurrentFileTheSail);

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