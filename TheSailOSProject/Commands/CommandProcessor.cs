using System;
using System.Collections.Generic;
using System.Linq;
using TheSailOSProject.Audio;
using TheSailOSProject.Commands.Applications;
using TheSailOSProject.Commands.Audio;
using TheSailOSProject.Commands.CPU;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.Commands.Disk;
using TheSailOSProject.Commands.Files;
using TheSailOSProject.Commands.Games;
using TheSailOSProject.Commands.Helpers;
using TheSailOSProject.Commands.Memory;
using TheSailOSProject.Commands.Network;
using TheSailOSProject.Commands.Power;
using TheSailOSProject.Commands.Processes;
using TheSailOSProject.Commands.Users;
using TheSailOSProject.FileSystem;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands;

public class CommandProcessor
{
    private readonly Dictionary<string, ICommand> _commands;
    private readonly ICommandHistoryManager _historyManager;
    private readonly IAliasManager _aliasManager;
    private readonly ILoginHandler _loginHandler;

    private List<string> _availableCommands = new List<string>
    {
        "ls", "dir", "cd", "mkdir", "rmdir", "renamedir", "copydir", "back", "create", "delete", "read", "write",
        "copy", "move", "rename", "info", "history", "clear", "help", "alias", "reboot", "shutdown", "pwd", "dns",
        "httpget", "ping", "memory", "freespace", "fstype", "log", "login", "createuser", "deleteuser", "listusers"
    };

    public CommandProcessor(
        IFileManager fileManager,
        IDirectoryManager directoryManager,
        ICommandHistoryManager historyManager,
        ICurrentDirectoryManager currentDirectoryManager,
        IRootDirectoryProvider rootDirectoryProvider,
        IVFSManager vfsManager,
        IDiskManager diskManager,
        IAudioManager audioManager,
        ILoginHandler loginHandler
    )
    {
        _historyManager = historyManager ?? throw new ArgumentNullException(nameof(historyManager));
        _aliasManager = new AliasManager(_availableCommands);
        _loginHandler = loginHandler ?? throw new ArgumentNullException(nameof(loginHandler));
        _commands = new Dictionary<string, ICommand>
        {
            { "ls", new ListFilesCommand(fileManager, directoryManager, currentDirectoryManager) },
            { "dir", new ListFilesCommand(fileManager, directoryManager, currentDirectoryManager) },
            { "cd", new ChangeDirectoryCommand(directoryManager, currentDirectoryManager, rootDirectoryProvider) },
            { "mkdir", new CreateDirectoryCommand(directoryManager, currentDirectoryManager) },
            { "rmdir", new DeleteDirectoryCommand(directoryManager, currentDirectoryManager, rootDirectoryProvider) },
            { "renamedir", new RenameDirectoryCommand(directoryManager, currentDirectoryManager) },
            { "copydir", new CopyDirectoryCommand(directoryManager, currentDirectoryManager) },
            { "back", new BackCommand(currentDirectoryManager, rootDirectoryProvider) },
            { "create", new CreateFileCommand(fileManager, currentDirectoryManager) },
            { "delete", new DeleteFileCommand(fileManager, currentDirectoryManager) },
            { "read", new ReadFileCommand(fileManager, currentDirectoryManager) },
            { "write", new WriteFileCommand(fileManager, currentDirectoryManager) },
            { "copy", new CopyFileCommand(fileManager, currentDirectoryManager) },
            { "move", new MoveFileCommand(fileManager, currentDirectoryManager) },
            { "rename", new RenameFileCommand(fileManager, currentDirectoryManager) },
            { "history", new ShowHistoryCommand(historyManager) },
            { "clear", new ClearHistoryCommand(historyManager) },
            { "help", new HelpCommand(GetHelpTexts()) },
            { "alias", new AliasCommand(_aliasManager) },
            { "pwd", new PrintWorkingDirectoryCommand(currentDirectoryManager) },
            { "reboot", new RebootCommand() },
            { "shutdown", new ShutdownCommand() },

            { "dns", new DnsCommand() },
            { "ping", new PingCommand() },
            { "netshutdown", new NetworkShutdownCommand() },
            { "netconfig", new NetworkConfigureCommand() },
            { "netstatus", new NetworkStatusCommand() },
            { "tcpserver", new TcpServerCommand() },
            { "tcpclient", new TcpClientCommand() },
            { "udpserver", new UdpServerCommand() },
            { "udpclient", new UdpClientCommand() },
            //{ "httpget", new HttpGetCommand() },

            { "memory", new MemoryCommand() },
            { "cpu", new CPUCommand() },
            { "processinfo", new ProcessInfoCommand() },

            { "freespace", new FreeSpaceCommand(vfsManager) },
            { "fstype", new FileSystemTypeCommand(vfsManager) },

            { "date", new DateCommand() },
            { "time", new TimeCommand() },

            { "format", new FormatDriveCommand(diskManager) },
            { "partition", new CreatePartitionCommand(diskManager) },
            { "partinfo", new ListPartitionsCommand(diskManager) },
            { "partman", new PartitionManagerCommand(diskManager) },

            { "playaudio", new PlayAudioCommand(audioManager, currentDirectoryManager) },
            { "stopaudio", new StopAudioCommand(audioManager, currentDirectoryManager) },

            { "snake", new SnakeGameCommand() },
            { "tetris", new TetrisGameCommand() },
            { "tictactoe", new TicTacToeGameCommand() },

            { "login", new LoginCommand(_loginHandler) },
            { "createuser", new CreateUserCommand() },
            { "deleteuser", new DeleteUserCommand() },
            { "listusers", new ListUsersCommand() },

            { "calculator", new CalculatorCommand() },
            { "textedit", new TextEditorCommand(fileManager) }
        };
    }

    public void ProcessCommand(string input)
    {
        input = input?.Trim();
        if (string.IsNullOrEmpty(input)) return;

        _historyManager.AddCommand(input);

        var parts = input.Split(' ');
        if (parts.Length == 0) return;

        var commandName = parts[0].ToLower();
        var args = parts.Skip(1).ToArray();

        if (string.IsNullOrEmpty(commandName))
        {
            Console.WriteLine("Error: Command cannot be empty.");
            return;
        }

        commandName = _aliasManager.GetCommand(commandName);

        if (_commands.ContainsKey(commandName))
        {
            _commands[commandName].Execute(args);
        }
        else
        {
            Console.WriteLine($"Unknown command: {commandName}");
        }
    }

    private Dictionary<string, string> GetHelpTexts()
    {
        return new Dictionary<string, string>
        {
            // File System Commands
            { "FILE SYSTEM COMMANDS", "The following commands are used to manage files and directories:" },
            { "ls", "Lists files and directories in the current directory or specified path.\nUsage: ls [path]" },
            { "dir", "Lists files and directories in the current directory or specified path.\nUsage: dir [path]" },
            { "cd", "Changes the current directory.\nUsage: cd <path>" },
            { "mkdir", "Creates a new directory.\nUsage: mkdir <path>" },
            { "rmdir", "Deletes a directory.\nUsage: rmdir <path>" },
            { "renamedir", "Renames a directory.\nUsage: renamedir <path> <newName>" },
            { "copydir", "Copies a directory from source to destination.\nUsage: copydir <source> <destination>" },
            { "back", "Moves to the parent directory.\nUsage: back" },
            { "create", "Creates a new file.\nUsage: create <path>" },
            { "delete", "Deletes a file.\nUsage: delete <path>" },
            { "read", "Reads and displays the content of a file.\nUsage: read <path>" },
            { "write", "Writes content to a file.\nUsage: write <path> <content>" },
            { "copy", "Copies a file from source to destination.\nUsage: copy <source> <destination>" },
            { "move", "Moves a file from source to destination.\nUsage: move <source> <destination>" },
            { "rename", "Renames a file.\nUsage: rename <oldPath> <newName>" },

            // History Commands
            { "HISTORY COMMANDS", "The following commands are used to manage command history:" },
            { "history", "Displays the command history.\nUsage: history" },
            { "clear", "Clears the command history.\nUsage: clear" },

            // System Commands
            { "SYSTEM COMMANDS", "The following commands are used to manage the system:" },
            { "help", "Displays help information for a command or lists available commands.\nUsage: help [command]" },
            { "alias", "Creates an alias for a command.\nUsage: alias <new_alias> <command>" },
            { "pwd", "Prints the current working directory.\nUsage: pwd" },
            { "reboot", "Reboots the system.\nUsage: reboot" },
            { "shutdown", "Shuts down the system.\nUsage: shutdown" },
            { "date", "Shows the current date.\nUsage: date" },
            { "time", "Shows the current time.\nUsage: time" },
            { "log", "Logs a message to the system log.\nUsage: log <message>" },
            { "login", "Logs in a user.\nUsage: login <username> <password>" },
            { "createuser", "Creates a new user.\nUsage: createuser <username> <password>" },
            { "deleteuser", "Deletes a user.\nUsage: deleteuser <username>" },
            { "listusers", "Lists all users.\nUsage: listusers" },

            // Network Commands
            { "NETWORK COMMANDS", "The following commands are used to manage the network:" },
            { "dns", "Performs a DNS lookup for the specified domain.\nUsage: dns <domain>" },
            { "httpget", "Retrieves the content of a web page.\nUsage: httpget <url>" },
            { "ping", "Pings the specified IP address.\nUsage: ping <ip_address>" },
            { "netshutdown", "Shuts down the network.\nUsage: netshutdown" },
            {
                "netconfig",
                "Configures the network settings.\nUsage:\nnetconfig - Configure with default settings (192.168.1.69)\nnetconfig <ip_address> <subnet_mask> <gateway> - Configure with custom settings\nExample: netconfig 192.168.1.69 255.255.255.0 192.168.1.254"
            },
            { "netstatus", "Displays network status.\nUsage: netstatus" },
            { "tcpserver", "Starts a TCP server.\nUsage: tcpserver" },
            {
                "tcpclient",
                @"Starts a TCP client and connects to a remote server. Usage: tcpclient <destination_ip> <destination_port> <local_port> <data> [timeout] Parameters:
  destination_ip   - Remote server IP address (e.g. 192.168.1.100)
  destination_port - Remote server port number (e.g. 80)
  local_port      - Local port number to use (e.g. 8080)
  data            - Data to send to the server
  timeout         - Optional connection timeout in ms (default: 80)
Examples:
  tcpclient 192.168.1.100 80 8080 ""GET / HTTP/1.1""
  tcpclient 10.0.0.1 8080 9000 ""Hello Server"" 120"
            },
            {
                "udpserver", "Starts a UDP server.\n" +
                             "Usage: udpserver [port]\n" +
                             "Parameters:\n" +
                             "  port - Port number to listen on (default: 8080)\n" +
                             "Example:\n" +
                             "  udpserver\n" +
                             "  udpserver 9000"
            },
            {
                "udpclient", @"Starts a UDP client and connects to a remote server.

Usage: udpclient <destination_ip> <destination_port> <local_port> <message> [timeout]

Parameters:
  destination_ip   - Remote server IP address (e.g. 192.168.1.100)
  destination_port - Remote server port number (e.g. 8080)
  local_port      - Local port number to use (e.g. 9000)
  message         - Message to send to the server
  timeout         - Optional connection timeout in ms (default: 80)

Examples:
  udpclient 192.168.1.100 8080 9000 ""Hello Server""
  udpclient 10.0.0.1 8080 9000 ""Test Message"" 120"
            },


            // Hardware Commands
            { "HARDWARE COMMANDS", "The following commands are used to display hardware information:" },
            { "memory", "Displays memory information.\nUsage: memory" },
            { "cpu", "Display CPU information." },

            // Process Commands
            { "PROCESS COMMANDS", "The following commands are used to manage processes:" },
            { "processinfo", "Display process information.\nUsage: processinfo [id <ID> | name <Name>]" },

            // Disk Commands
            { "DISK COMMANDS", "The following commands are used to manage disks and partitions:" },
            { "freespace", "Displays available free space on a drive.\nUsage: freespace <drive>" },
            { "fstype", "Displays the file system type of a drive.\nUsage: fstype <drive>" },
            { "format", "Formats a drive.\nUsage: format <drive_letter>" },
            { "partition", "Creates a partition on a drive.\nUsage: partition <drive_letter> <size>" },
            { "partinfo", "Lists information about partitions on a drive.\nUsage: partinfo <drive_letter>" },
            { "partman", "Opens the partition manager.\nUsage: partman" },

            // Audio Commands
            { "AUDIO COMMANDS", "The following commands are used to manage audio:" },
            { "playaudio", "Plays an audio file.\nUsage: playaudio <path>" },
            { "stopaudio", "Stops the currently playing audio.\nUsage: stopaudio" },

            // Game Commands
            { "GAME COMMANDS", "The following commands are used to play games:" },
            { "snake", "Play Snake game.\nUse arrow keys to move, ESC to exit." },
            { "tetris", "Play Tetris game.\nUse arrow keys to move, ESC to exit." },
            { "tictactoe", "Play Tic-Tac-Toe against the computer.\nUse numpad (1-9) to place X." },

            //Application Commands
            { "APPLICATION COMMANDS", "The following commands are used to manage applications:" },
            { "calculator", "Opens the calculator application.\nUsage: calculator" },
            { "textedit", "Opens the text editor application.\nUsage: textedit 0:\\test.txt" }
        };
    }
}