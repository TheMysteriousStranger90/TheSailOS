﻿using System;
using CosmosFtpServer;
using TheSailOSProject.Audio;
using TheSailOSProject.Commands;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.Commands.Helpers;
using TheSailOSProject.FileSystem;
using TheSailOSProject.Hardware.Memory;
using TheSailOSProject.Network;
using TheSailOSProject.Processes;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;
using Sys = Cosmos.System;

namespace TheSailOSProject
{
    public class Kernel : Sys.Kernel, ILoginHandler
    {
        private TheSailFileSystem _fileSystem;
        private CommandProcessor _commandProcessor;
        private User _loggedInUser = null;
        private ICommandHistoryManager _historyManager;
        private IAliasManager _aliasManager;
        private ICurrentDirectoryManager _currentDirectoryManager;
        private IRootDirectoryProvider _rootDirectoryProvider;
        private IAudioManager _audioManager;
        
        protected override void BeforeRun()
        {
            ConsoleManager.Initialize();
            System.Threading.Thread.Sleep(100);
            
            InitializeFileSystem();
            ConsoleManager.WriteLineColored("[FileSystem] System initialized", ConsoleStyle.Colors.Success);

            InitializeProcessManager();
            ConsoleManager.WriteLineColored("[ProcessManager] System initialized", ConsoleStyle.Colors.Success);

            InitializeAudio();
            ConsoleManager.WriteLineColored("[Audio] System initialized", ConsoleStyle.Colors.Success);

            InitializeMemoryManager();
            ConsoleManager.WriteLineColored("[MemoryManager] System initialized", ConsoleStyle.Colors.Success);
            
            InitializeNetwork();
            ConsoleManager.WriteLineColored("[Network] System initialized", ConsoleStyle.Colors.Success);

            InitializeCommandProcessor();
            ConsoleManager.WriteLineColored("[CommandProcessor] System initialized", ConsoleStyle.Colors.Success);

            UserManager.Initialize();
        }

        protected override void Run()
        {
            try
            {
                if (_loggedInUser == null)
                {
                    PromptLogin();
                }
                else
                {
                    ShowCommandPrompt();
                }
            }
            catch (Exception ex)
            {
                ShowErrorScreen(ex);
            }
        }

        protected override void OnBoot()
        {
            Sys.Global.Init(GetTextScreen(), true, true, true, true);
        }

        private void PromptLogin()
        {
            ConsoleManager.WriteLineColored("Please log in to continue.", ConsoleStyle.Colors.Warning);

            ConsoleManager.WriteColored("Username: ", ConsoleStyle.Colors.Primary);
            string username = Console.ReadLine();

            ConsoleManager.WriteColored("Password: ", ConsoleStyle.Colors.Primary);
            string password = Console.ReadLine();

            string loginCommand = $"login {username} {password}";
            _commandProcessor.ProcessCommand(loginCommand);

            if (_loggedInUser == null)
            {
                PromptLogin();
            }
            else
            {
                ShowCommandPrompt();
            }
        }

        private void ShowCommandPrompt()
        {
            ConsoleManager.WriteColored($"{_currentDirectoryManager.GetCurrentDirectory()}",
                ConsoleStyle.Colors.Primary);
            ConsoleManager.WriteColored(ConsoleStyle.Symbols.Prompt + " ", ConsoleStyle.Colors.Primary);

            var input = Console.ReadLine();

            if (!string.IsNullOrEmpty(input))
            {
                ProcessCommand(input);
            }

            ShowCommandPrompt();
        }

        private void ProcessCommand(string input)
        {
            string[] parts = input.Split(' ');
            string commandName = parts[0].ToLower();

            if ((commandName == "createuser" || commandName == "deleteuser" || commandName == "listusers") &&
                (_loggedInUser == null || _loggedInUser.Type != UserType.Administrator))
            {
                ConsoleManager.WriteLineColored("Insufficient permissions.", ConsoleStyle.Colors.Error);
                return;
            }

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
                _audioManager,
                this
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

        private void InitializeProcessManager()
        {
            Console.WriteLine("Initializing Process Manager...");

            ProcessManager.Initialize();

            var memoryService = new MemoryManagementService();
            ProcessManager.Register(memoryService);
            ProcessManager.Start(memoryService);
            ProcessManager.Update();
        }
        
        private void InitializeFtpServer()
        {
            Console.WriteLine("Initializing FTP Server...");
            
            _rootDirectoryProvider = new RootDirectoryProvider();
            _currentDirectoryManager = new CurrentDirectoryManager(_rootDirectoryProvider.GetRootDirectory());
            
            ConsoleManager.WriteLineColored("Starting FTP server...", ConsoleStyle.Colors.Primary);

            var ftpServer = new FtpServer(_fileSystem, _currentDirectoryManager.GetCurrentDirectory());
            Console.WriteLine("Listening...");
            ftpServer.Listen();

            Console.WriteLine("Client connected.");
        }

        public void OnLoginSuccess(User user)
        {
            _loggedInUser = user;
            ConsoleManager.WriteLineColored($"Welcome, {user.Username}!", ConsoleStyle.Colors.Success);
        }
        
        static void ShowErrorScreen(Exception ex)
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("A fatal error has occurred:");
            Console.WriteLine(ex.Message);
            Console.WriteLine("Press any key to reboot...");
            Console.ReadKey();
            Sys.Power.Reboot();
        }
    }
}