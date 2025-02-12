using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using TheSailOSProject.Commands;
using TheSailOSProject.FileSystem;
using TheSailOSProject.Memory;
using Sys = Cosmos.System;

namespace TheSailOSProject
{
   public class Kernel : Sys.Kernel
    {
        private TheSailFileSystem _fileSystem;
        private CommandProcessor _commandProcessor;
        private CommandHistoryManager _historyManager;
        private AliasManager _aliasManager;
        private CurrentDirectoryManager _currentDirectoryManager;
        private RootDirectoryProvider _rootDirectoryProvider;

        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully.");
            
            InitializeMemoryManager();
            Console.WriteLine("[MemoryManager] Initialized");

            InitializeFileSystem();
            Console.WriteLine("[FileSystem] System initialized");
            
            InitializeNetwork();
            Console.WriteLine("[Network] System initialized");

            InitializeCommandProcessor();
            Console.WriteLine("[CommandProcessor] Initialized");
        }

        protected override void Run()
        {
            Console.Write($"{_currentDirectoryManager.GetCurrentDirectory()}> ");
            var input = Console.ReadLine();
            _commandProcessor.ProcessCommand(input);
        }

        private void InitializeFileSystem()
        {
            Console.WriteLine("Initializing file system...");

            _fileSystem = new TheSailFileSystem();
            _fileSystem.Initialize();
        }
        
        private void InitializeMemoryManager()
        {
            Console.WriteLine("Initializing Memory Manager...");
            
            MemoryManager.Initialize();
        }

        private void InitializeCommandProcessor()
        {
            Console.WriteLine("Initializing Command Processor...");

            _historyManager = new CommandHistoryManager();
            _rootDirectoryProvider = new RootDirectoryProvider();
            _currentDirectoryManager = new CurrentDirectoryManager(_rootDirectoryProvider.GetRootDirectory());

            _commandProcessor = new CommandProcessor(
                _fileSystem,
                _fileSystem,
                _historyManager,
                _currentDirectoryManager,
                _rootDirectoryProvider
            );
        }
        
        private void InitializeNetwork()
        {
            try
            {
                using (var dhcp = new DHCPClient())
                {
                    Console.WriteLine("Sending DHCP Discover packet...");
                    if (dhcp.SendDiscoverPacket() != -1)
                    {
                        Console.WriteLine($"IP Address: {NetworkConfiguration.CurrentAddress}");
                    }
                    else
                    {
                        Console.WriteLine("[WARNING] DHCP failed. Please configure network manually.");
                        
                        NetworkDevice nic = NetworkDevice.GetDeviceByName("eth0");
                        if (nic != null)
                        {
                            IPConfig.Enable(nic, new Address(192, 168, 1, 69), new Address(255, 255, 255, 0), new Address(192, 168, 1, 254));
                            Console.WriteLine($"Static IP configuration applied: IP={NetworkConfiguration.CurrentAddress}");
                        }
                        else
                        {
                            Console.WriteLine("[ERROR] Network device 'eth0' not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Network initialization failed: {ex.Message}");
            }
        }
    }
}
