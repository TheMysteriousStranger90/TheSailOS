using System;
using Cosmos.System.Graphics;
using TheSailOS.CommandsTheSailTwo;
using TheSailOS.FileSystemTheSail;
using TheSailOS.MemoryTheSail;
using TheSailOS.NetworkTheSail;
using TheSailOS.ProcessTheSail;
using Sys = Cosmos.System;

namespace TheSailOS
{
    public class Kernel : Sys.Kernel
    {
        private FileSystem _fileSystem;
        private CommandProcessor _commandProcessor;
        
        
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

        public static void SetCurrentDirectory(string path)
        {
            CurrentDirectory = path;
        }

        protected override void BeforeRun()
        {
            Console.WriteLine("Initializing TheSail OS...");
            InitializeMemoryManagement();
            Console.WriteLine("[Memory] System initialized");

            // 2. Process Management
            InitializeProcessManagement();
            Console.WriteLine("[Process] System initialized");
            
            // 3. File System
            InitializeFileSystem();
            Console.WriteLine("[FileSystem] System initialized");
            
            InitializeNetwork();
            Console.WriteLine("[Network] System initialized");
            
        }
        
        private void InitializeFileSystem()
        {
            Console.WriteLine("Cosmos booted successfully. Initializing file system...");
            
            _fileSystem = new ();
            _fileSystem.Initialize();
            
            _commandProcessor = new CommandProcessor(_fileSystem);
            
            Console.WriteLine("File system initialized.");
        }
        
        protected override void Run()
        {
            Console.Write(">");
            string input = Console.ReadLine();
            _commandProcessor.ProcessCommand(input);
            
            Run();
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
                if (!NetworkDeviceManager.HasNetworkDevice())
                {
                    Console.WriteLine("[Network] No network devices found");
                    return;
                }

                _networkService = NetworkService.Instance;
                _networkCommandHandler = new NetworkCommandHandler();
                
                var networkStatus = new NetworkStatus();
                
                _networkService.SetStatusCallback(status => 
                {
                    Console.WriteLine($"[Network] {status}");
                    
                    networkStatus.IsConnected = NetworkManager.IsNetworkAvailable();
                    //NetworkConfiguration.CurrentAddress;
                });
                
                if (_networkService.Initialize())
                {
                    Console.WriteLine("[Network] System initialized");
                    Console.WriteLine(networkStatus.ToString());
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Network] Init failed: {ex.Message}");
            }
        }

        private void InitializeFtpServer()
        {
            try 
            {
                var ftpServer = new FtpServer(Kernel.CurrentFileTheSail._vfs, @"0:\FTP");
                ftpServer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FTP] Init failed: {ex.Message}");
            }
        }

        
        
        
        /*
        protected override void Run()
        {
            Console.Write($"{CurrentDirectory}> ");
            var input = Console.ReadLine();

            _commandProcessor.ProcessCommand(input);
            
            _processManager.UpdateProcessState();
        }
        */
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