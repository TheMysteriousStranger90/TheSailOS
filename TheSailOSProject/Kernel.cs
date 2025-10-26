using System;
using CosmosFtpServer;
using TheSailOSProject.Audio;
using TheSailOSProject.Commands;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.Commands.Helpers;
using TheSailOSProject.FileSystem;
using TheSailOSProject.Hardware.Memory;
using TheSailOSProject.Hardware.Timer;
using TheSailOSProject.Logging;
using TheSailOSProject.Network;
using TheSailOSProject.Permissions;
using TheSailOSProject.Processes;
using TheSailOSProject.Session;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;
using Sys = Cosmos.System;

namespace TheSailOSProject
{
    public class Kernel : Sys.Kernel, ILoginHandler, ILogoutHandler
    {
        private TheSailFileSystem _fileSystem;
        private CommandProcessor _commandProcessor;
        private MemoryManagementService _memoryService;
        private SessionCleanupService _sessionCleanupService;
        private User _loggedInUser = null;
        private Session.Session _currentSession = null;
        private ICommandHistoryManager _historyManager;
        private ICurrentDirectoryManager _currentDirectoryManager;
        private IRootDirectoryProvider _rootDirectoryProvider;
        private IAudioManager _audioManager;
        public static User CurrentUser { get; set; }

        protected override void BeforeRun()
        {
            ConsoleManager.Initialize();
            System.Threading.Thread.Sleep(100);

            InitializeFileSystem();
            ConsoleManager.WriteLineColored("[FileSystem] System initialized", ConsoleStyle.Colors.Success);

            InitializePermissionsManager();
            ConsoleManager.WriteLineColored("[Permissions] System initialized", ConsoleStyle.Colors.Success);

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

            InitializeUserManager();
            ConsoleManager.WriteLineColored("[UserManager] System initialized", ConsoleStyle.Colors.Success);

            InitializeLog();
            ConsoleManager.WriteLineColored("[Logging] System initialized", ConsoleStyle.Colors.Success);
            
            InitializeTimers();
            ConsoleManager.WriteLineColored("[TimerManager] System initialized", ConsoleStyle.Colors.Success);

            StartSystemServices();
            ConsoleManager.WriteLineColored("[Services] System services started", ConsoleStyle.Colors.Success);
        }

        protected override void Run()
        {
            try
            {
                while (true)
                {
                    if (_loggedInUser == null)
                    {
                        PromptLogin();
                    }
                    else
                    {
                        ProcessUserSession();
                    }
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

        private void ProcessUserSession()
        {
            while (_loggedInUser != null)
            {
                ShowCommandPrompt();
                var input = Console.ReadLine();

                if (!string.IsNullOrEmpty(input))
                {
                    ProcessCommand(input);

                    if (_loggedInUser == null)
                        return;
                }
            }
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
            SessionManager.UpdateSessionActivity(_currentSession?.SessionId);
            ConsoleManager.WriteColored(
                $"{_currentDirectoryManager.GetCurrentDirectory()}",
                ConsoleStyle.Colors.Primary
            );
            ConsoleManager.WriteColored(
                $"{ConsoleStyle.Symbols.Prompt} ",
                ConsoleStyle.Colors.Primary
            );
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
                this,
                this
            );
        }

        private void InitializeNetwork()
        {
            Console.WriteLine("Initializing network...");

            NetworkManager.Initialize();
        }
        
        private void InitializePermissionsManager()
        {
            Console.WriteLine("Initializing Permissions Manager...");

            PermissionsManager.Initialize();
        }
        
        private void InitializeUserManager()
        {
            Console.WriteLine("Initializing User Manager...");

            UserManager.Initialize();
        }
        
        private void InitializeLog()
        {
            Console.WriteLine("Initializing Logging System...");

            Log.Initialize();
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
            
            //ProcessManager.Update();
        }

        private void InitializeTimers()
        {
            Console.WriteLine("Initializing Timer Manager...");
            TimerManager.Initialize();
            
            TimerManager.CreateTimer("session_cleanup", 60000000000, SessionCleanupCallback, true);
        }
        
        private void StartSystemServices()
        {
            Console.WriteLine("Starting system services...");

            _memoryService = new MemoryManagementService();
            ProcessManager.Register(_memoryService);
            ProcessManager.Start(_memoryService);

            _sessionCleanupService = new SessionCleanupService();
            ProcessManager.Register(_sessionCleanupService);
            ProcessManager.Start(_sessionCleanupService);

            Console.WriteLine($"[Services] Started {ProcessManager.GetProcesses().Count} system services");
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
            CurrentUser = user;
            _currentSession = SessionManager.StartSession(user);
            Log.WriteLog(LogPriority.Info, "Authentication", $"User '{user.Username}' logged in successfully",
                "SYSTEM");
            ConsoleManager.WriteLineColored($"Welcome, {user.Username}!", ConsoleStyle.Colors.Success);
        }

        public void OnLogout()
        {
            if (_loggedInUser == null) return;

            var sessionId = _currentSession?.SessionId;
            if (!string.IsNullOrEmpty(sessionId))
            {
                Log.WriteLog(LogPriority.Info, "Authentication",
                    $"Session {sessionId} terminated for user '{CurrentUser.Username}'", "SYSTEM");

                SessionManager.EndSession(sessionId);
                Console.WriteLine($"[Kernel] Session {sessionId} terminated");
            }

            _loggedInUser = null;
            CurrentUser = null;
            _currentSession = null;
            Console.Clear();

            ConsoleManager.WriteLineColored("Successfully logged out.", ConsoleStyle.Colors.Success);

            Cosmos.Core.Memory.Heap.Collect();
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
        
        private void SessionCleanupCallback()
        {
            SessionManager.CleanupInactiveSessions(TimeSpan.FromMinutes(30));
        }
    }
}