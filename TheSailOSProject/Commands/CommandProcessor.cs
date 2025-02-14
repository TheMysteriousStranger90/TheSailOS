using System;
using System.Collections.Generic;
using System.Linq;
using TheSailOSProject.Audio;
using TheSailOSProject.Commands.Audio;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.Commands.Disk;
using TheSailOSProject.Commands.Files;
using TheSailOSProject.Commands.Games;
using TheSailOSProject.Commands.Helpers;
using TheSailOSProject.Commands.Memory;
using TheSailOSProject.Commands.Network;
using TheSailOSProject.Commands.Power;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands;

public class CommandProcessor

{
    private readonly Dictionary<string, ICommand> _commands;
    private readonly ICommandHistoryManager _historyManager;
    private readonly IAliasManager _aliasManager;

    private List<string> _availableCommands = new List<string>
    {
        "ls", "dir", "cd", "mkdir", "rmdir", "renamedir", "copydir", "back", "create", "delete", "read", "write",
        "copy", "move", "rename", "info", "history", "clear", "help", "alias", "reboot", "shutdown", "pwd", "dns",
        "httpget", "ping", "memory", "freespace", "fstype"
    };
    
    public CommandProcessor(
        IFileManager fileManager,
        IDirectoryManager directoryManager,
        ICommandHistoryManager historyManager,
        ICurrentDirectoryManager currentDirectoryManager,
        IRootDirectoryProvider rootDirectoryProvider,
        IVFSManager vfsManager,
        IDiskManager diskManager,
        IAudioManager audioManager
        )
    {
        _historyManager = historyManager ?? throw new ArgumentNullException(nameof(historyManager));
        _aliasManager = new AliasManager(_availableCommands);
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
            { "memory", new MemoryCommand() },
            { "freespace", new FreeSpaceCommand(vfsManager) },
            { "fstype", new FileSystemTypeCommand(vfsManager) },
            
            { "date", new DateCommand() },
            { "time", new TimeCommand() },

            { "format", new FormatDriveCommand(diskManager) },
            { "partition", new CreatePartitionCommand(diskManager) },
            { "partinfo", new ListPartitionsCommand(diskManager) },
            { "partman", new PartitionManagerCommand(diskManager) },
            
            {"playaudio", new PlayAudioCommand(audioManager, currentDirectoryManager)},
            {"stopaudio", new StopAudioCommand(audioManager, currentDirectoryManager)},
            
            {"snake", new SnakeGameCommand()},
            {"tetris", new TetrisGameCommand()},
            {"tictactoe", new TicTacToeGameCommand()}
            
            //{ "httpget", new HttpGetCommand() },
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
            { "history", "Displays the command history.\nUsage: history" },
            { "clear", "Clears the command history.\nUsage: clear" },
            { "help", "Displays help information for a command or lists available commands.\nUsage: help [command]" },
            { "alias", "Creates an alias for a command.\nUsage: alias <new_alias> <command>" },
            { "pwd", "Prints the current working directory.\nUsage: pwd" },
            { "reboot", "Reboots the system.\nUsage: reboot" },
            { "shutdown", "Shuts down the system.\nUsage: shutdown" },
            { "dns", "Performs a DNS lookup for the specified domain.\nUsage: dns <domain>" },
            { "httpget", "Retrieves the content of a web page.\nUsage: httpget <url>" },
            { "ping", "Pings the specified IP address.\nUsage: ping <ip_address>" },
            {"netshutdown", "Shuts down the network.\nUsage: netshutdown"},
            {"netconfig", "Configures the network.\nUsage: netconfig"},
            {"netstatus", "Displays network status.\nUsage: netstatus"},
            { "memory", "Displays memory information.\nUsage: memory" },
            { "freespace", "Displays available free space on a drive.\nUsage: freespace <drive>" },
            { "fstype", "Displays the file system type of a drive.\nUsage: fstype <drive>" },
            { "date", "Shows the current date.\nUsage: date" },
            { "time", "Shows the current time.\nUsage: time" },
            { "format", "Formats a drive.\nUsage: format <drive_letter>" },
            { "partition", "Creates a partition on a drive.\nUsage: partition <drive_letter> <size>" },
            { "partinfo", "Lists information about partitions on a drive.\nUsage: partinfo <drive_letter>" },
            { "partman", "Opens the partition manager.\nUsage: partman" },
            { "playaudio", "Plays an audio file.\nUsage: playaudio <path>" },
            { "stopaudio", "Stops the currently playing audio.\nUsage: stopaudio" },
            { "snake", "Play Snake game.\nUse arrow keys to move, ESC to exit." },
            { "tetris", "Play Tetris game.\nUse arrow keys to move, ESC to exit." },
            { "tictactoe", "Play Tic-Tac-Toe against the computer.\nUse numpad (1-9) to place X." }
        };
    }
}
