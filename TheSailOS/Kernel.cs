using System;
using System.Drawing;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.Graphics;
using TheSailOS.Commands;
using TheSailOS.Configuration;
using TheSailOS.FileSystemTheSail;
using TheSailOS.MemoryTheSail;
using TheSailOS.NetworkTheSail;
using TheSailOS.ProcessTheSail;
using Sys = Cosmos.System;

namespace TheSailOS
{
    public class Kernel : Sys.Kernel
    {
        private Canvas _canvas;
        private NetworkService _networkService;
        private NetworkCommandHandler _networkCommandHandler;
        private FtpServer _ftpServer;
        
        private ProcessManager _processManager;
        private EnhancedMemoryManager _memoryManager;
        private VirtualMemoryManager _virtualMemoryManager;
        public static string CurrentDirectory { get; private set; } = @"0:\";
        public static string VersionOs = "0.0.1";
        private bool _isRunning;
        public static FileTheSail CurrentFileTheSail;

        private CommandProcessor _commandProcessor;

        public static void SetCurrentDirectory(string path)
        {
            CurrentDirectory = path;
        }

        protected override void BeforeRun()
        {
            Console.WriteLine("Initializing TheSail OS...");
            
            InitializeMemoryManagement();
            InitializeProcessManagement();
            InitializeFileSystem();
        }
        
        private void InitializeFileSystem()
        {
            TheSailOSCfg.Load();
            CurrentFileTheSail = new FileTheSail();
            VFSManager.RegisterVFS(CurrentFileTheSail._vfs);

            var fileReader = new FileReader(CurrentFileTheSail);
            var fileWriter = new FileWriter(CurrentFileTheSail);
            var fileMover = new FileMover(CurrentFileTheSail, fileReader, fileWriter);
            var fileSystemOperations = new FileSystemOperations(CurrentFileTheSail);

            _commandProcessor = new CommandProcessor(
                fileReader, 
                fileWriter, 
                fileMover, 
                fileSystemOperations,
                _processManager);
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
                if (_isRunning)
                {
                    MonitorSystemResources();
                }
            }, 10);

            // Memory Manager Process
            _processManager.CreateProcess("MemoryManager", () =>
            {
                if (_isRunning)
                {
                    ManageSystemMemory();
                }
            }, 8);
        }
        
        private void ManageSystemMemory()
        {
            try
            {
                // Custom memory cleanup instead of GC.Collect()
                _memoryManager.CleanupUnusedMemory();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Memory management error: {ex.Message}");
            }
        }
        
        private void InitializeMemoryManagement()
        {
            _memoryManager = new EnhancedMemoryManager();
            // Remove VirtualMemoryManager for now
            _memoryManager.AllocateMemory(1024 * 1024, MemoryPermissions.Read | MemoryPermissions.Write);
        }
        
        private void MonitorSystemResources()
        {
            try
            {
                _processManager.UpdateProcessState();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Monitor error: {ex.Message}");
            }
        }
        
        private void InitializeNetwork()
        {
            try
            {
                _networkService = NetworkService.Instance;
                _networkCommandHandler = new NetworkCommandHandler();
            
                if (_networkService.Initialize())
                {
                    Console.WriteLine("[Network] System initialized");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Network] Init failed: {ex.Message}");
            }
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

/*

    protected override void BeforeRun()
    {
        Console.WriteLine("Initializing TheSail OS...");
        
        // 1. Memory Management
        InitializeMemoryManagement();
        Console.WriteLine("[Memory] System initialized");

        // 2. Process Management
        InitializeProcessManagement();
        Console.WriteLine("[Process] System initialized");

        // 3. File System
        TheSailOSCfg.Load();
        CurrentFileTheSail = new FileTheSail();
        VFSManager.RegisterVFS(CurrentFileTheSail._vfs);
        InitializeFileSystem();
        Console.WriteLine("[FileSystem] System initialized");

        // 4. System Processes
        InitializeSystemProcesses();
        Console.WriteLine("[System] Processes initialized");

        // 5. Network
        InitializeNetwork();
        
        // Set system as running
        _isRunning = true;
        
        Console.WriteLine($"TheSail OS {VersionOs} booted successfully.");
        Console.WriteLine("Type 'help' for available commands.");
    }

    protected override void Run()
    {
        if (!_isRunning) return;

        Console.Write($"{CurrentDirectory}> ");
        var input = Console.ReadLine();

        _commandProcessor.ProcessCommand(input);
        _processManager.UpdateProcessState();
    }
    */