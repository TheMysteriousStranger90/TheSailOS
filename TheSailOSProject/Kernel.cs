using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using TheSailOSProject.Audio;
using TheSailOSProject.Commands;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.Commands.Helpers;
using TheSailOSProject.FileSystem;
using TheSailOSProject.Hardware.Memory;
using TheSailOSProject.Network;
using TheSailOSProject.Styles;
using Sys = Cosmos.System;

namespace TheSailOSProject
{
    public class Kernel : Sys.Kernel
    {
        private TheSailFileSystem _fileSystem;
        private CommandProcessor _commandProcessor;
        
        private ICommandHistoryManager _historyManager;
        private IAliasManager _aliasManager;
        private ICurrentDirectoryManager _currentDirectoryManager;
        private IRootDirectoryProvider _rootDirectoryProvider;
        private IAudioManager _audioManager;
        
        protected override void BeforeRun()
        {
            ConsoleManager.Initialize();
            System.Threading.Thread.Sleep(100);
            
            InitializeAudio();
            ConsoleManager.WriteLineColored("[Audio] System initialized", ConsoleStyle.Colors.Success);
            
            InitializeMemoryManager();
            ConsoleManager.WriteLineColored("[MemoryManager] System initialized", ConsoleStyle.Colors.Success);

            InitializeFileSystem();
            ConsoleManager.WriteLineColored("[FileSystem] System initialized", ConsoleStyle.Colors.Success);
            
            InitializeNetwork();
            ConsoleManager.WriteLineColored("[Network] System initialized", ConsoleStyle.Colors.Success);

            InitializeCommandProcessor();
            ConsoleManager.WriteLineColored("[CommandProcessor] System initialized", ConsoleStyle.Colors.Success);
        }

        protected override void Run()
        {
            ConsoleManager.WriteColored($"{_currentDirectoryManager.GetCurrentDirectory()}", ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteColored(ConsoleStyle.Symbols.Prompt + " ", ConsoleStyle.Colors.Primary);
            
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
                _rootDirectoryProvider,
                _fileSystem,
                _fileSystem,
                _audioManager
            );
        }

        private void InitializeNetwork()
        {
            Console.WriteLine("Initializing network...");
            
            NetworkManager.Initialize();
        }
        
        private void InitializeAudio()
        {
            try
            {
                Console.WriteLine("Initializing Audio System...");
        
                _audioManager = new AudioManager();
                _audioManager.Initialize();

                if (_audioManager.IsAudioEnabled)
                {
                    ConsoleManager.WriteLineColored("[Audio] System initialized successfully", 
                        ConsoleStyle.Colors.Success);
                }
                else
                {
                    ConsoleManager.WriteLineColored("[Audio] System initialized in fallback mode (no audio)", 
                        ConsoleStyle.Colors.Warning);
                }
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"[Audio] Initialization failed: {ex.Message}", 
                    ConsoleStyle.Colors.Error);
                _audioManager = null;
            }
        }
    }
}