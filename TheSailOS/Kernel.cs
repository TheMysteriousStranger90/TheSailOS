using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.System.FileSystem.VFS;
using TheSailOS.Commands;
using TheSailOS.Configuration;
using TheSailOS.FileSystem;
using TheSailOS.ProcessTheSail;
using Sys = Cosmos.System;

namespace TheSailOS
{
    public class Kernel : Sys.Kernel
    {
        private ProcessManager _processManager;
        public static string CurrentDirectory { get; private set; } = @"0:\";
        public static string VersionOs = "0.0.1";

        public static FileTheSail CurrentFileTheSail;

        private CommandProcessor _commandProcessor;

        public static void SetCurrentDirectory(string path)
        {
            CurrentDirectory = path;
        }

        protected override void BeforeRun()
        {
            TheSailOSCfg.Load();
            
            CurrentFileTheSail = new FileTheSail();
            VFSManager.RegisterVFS(CurrentFileTheSail._vfs);
            
            var fileReader = new FileReader(CurrentFileTheSail);
            var fileWriter = new FileWriter(CurrentFileTheSail);
            var fileMover = new FileMover(CurrentFileTheSail, fileReader, fileWriter);
            var fileSystemOperations = new FileSystemOperations(CurrentFileTheSail);

            _commandProcessor = new CommandProcessor(fileReader, fileWriter, fileMover, fileSystemOperations);
            
            InitializeProcessManagement();
            InitializeSystemProcesses();

            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
            Console.WriteLine("Type a command to execute.");
        }
        
        private void InitializeProcessManagement()
        {
            _processManager = new ProcessManager();
        }
        
        private void InitializeSystemProcesses()
        {
            // System Monitor Process
            _processManager.CreateProcess("SystemMonitor", () =>
            {

            }, 10);

            // Memory Manager Process
            _processManager.CreateProcess("MemoryManager", () =>
            {

            }, 8);
        }

        protected override void Run()
        {
            Console.Write($"{CurrentDirectory}> ");
            var input = Console.ReadLine();

            _commandProcessor.ProcessCommand(input);
            
            _processManager.UpdateProcessState();
        }
    }
}