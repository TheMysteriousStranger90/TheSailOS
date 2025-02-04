using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TheSailOS.FileSystemTheSail;
using TheSailOS.NetworkTheSail;
using TheSailOS.PowerSystem;
using TheSailOS.ProcessTheSail;

namespace TheSailOS.Commands;

public class CommandProcessor
{
    private readonly NetworkCommandHandler _networkHandler;
    private readonly FileReader _fileReader;
    private readonly FileWriter _fileWriter;
    private readonly FileMover _fileMover;
    private readonly FileSystemOperations _fileSystemOperations;
    private readonly CommandHistoryManager _historyManager;
    private readonly AliasManager _aliasManager;
    private readonly RebootCommand _rebootCommand;
    private readonly ShutdownCommand _shutdownCommand;
    private readonly ProcessManager _processManager;

    private List<string> _commandHistory = new List<string>();

    private List<string> _availableCommands = new List<string>
    {
        "create", "read", "write", "delete", "move", "mkdir", "rmdir", "ls", "mvdir", "help", "history", "alias",
        "batch", "rename", "forceremove", "forcecopy", "save", "list", "cd", "diskspace", "shutdown", "reboot", "pwd",
        "back", "netinit", "connect", "disconnect", "send", "ping", "ftpstart", "ftpstop", "ps", "kill", "priority", "pinfo", "pstat", "childproc"
    };

    private Dictionary<string, string> _commandAliases = new Dictionary<string, string>();

    public CommandProcessor(FileReader fileReader, FileWriter fileWriter, FileMover fileMover,
        FileSystemOperations fileSystemOperations, ProcessManager processManager)
    {
        this._fileReader = fileReader;
        this._fileWriter = fileWriter;
        this._fileMover = fileMover;
        this._fileSystemOperations = fileSystemOperations;
        this._rebootCommand = new RebootCommand();
        this._shutdownCommand = new ShutdownCommand();
        _historyManager = new CommandHistoryManager();
        _aliasManager = new AliasManager(_availableCommands);
        
        _networkHandler = new NetworkCommandHandler();
        _processManager = processManager ?? throw new ArgumentNullException(nameof(processManager));
    }


    public void ProcessCommand(string input)
    {
        input = input?.Trim();
        Console.WriteLine($"Debug: ProcessCommand called with input: '{input}'");

        _commandHistory.Add(input);

        var parts = input.Split(' ');
        if (parts.Length == 0) return;

        var command = parts[0].ToLower().Trim();
        var args = parts.Skip(1).ToArray();

        if (string.IsNullOrEmpty(command))
        {
            Console.WriteLine("Error: Command cannot be empty.");
            return;
        }

        if (_commandAliases.ContainsKey(command))
        {
            command = _commandAliases[command];
        }

        Console.WriteLine($"Debug: Command - {command}, Args - {string.Join(", ", args)}");

        _historyManager.AddCommand(input);
        command = _aliasManager.GetCommand(command);

        try
        {
            switch (command)
            {
                case "ls":
                    _fileSystemOperations.ListDirectory(args.Length > 0
                        ? args[0]
                        : CurrentPathManager.CurrentDirectory);
                    break;

                case "mvdir":
                    if (args.Length < 2)
                        throw new ArgumentException(
                            "Missing arguments for 'mvdir'. Usage: mvdir <source> <destination>");
                    _fileSystemOperations.MoveDirectory(args[0], args[1]);
                    break;
                case "history":
                    _historyManager.ShowHistory();
                    break;
                case "mkdir":
                    if (args.Length < 1)
                        throw new ArgumentException("Missing argument for 'mkdir'. Usage: mkdir <directory>");
                    _fileSystemOperations.CreateDirectory(args[0]);
                    Console.WriteLine($"Created directory {args[0]}");
                    break;
                case "create":
                    try
                    {
                        if (args.Length < 1)
                            throw new ArgumentException("Missing argument for 'create'. Usage: create <filename>");

                        string filename = args[0].Trim();
                        if (string.IsNullOrEmpty(filename))
                            throw new ArgumentException("Filename cannot be empty");

                        _fileWriter.WriteFile(filename, "");
                        Thread.Sleep(100); // Wait for filesystem to update
                        Console.WriteLine($"Created file: {filename}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating file: {ex.Message}");
                    }

                    break;
                case "read":
                    try
                    {
                        if (args.Length < 1)
                            throw new ArgumentException("Missing argument for 'read'. Usage: read <filename>");

                        var content = _fileReader.ReadFile(args[0]);
                        Console.WriteLine($"File content:\n{content}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading file: {ex.Message}");
                    }

                    break;
                case "write":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: write <filename> <content>");
                        break;
                    }

                    try
                    {
                        string filename = args[0];
                        string content = string.Join(" ", args.Skip(1));

                        Console.WriteLine($"Writing to file: {filename}");
                        Console.WriteLine($"Content: {content}");

                        _fileWriter.WriteFile(filename, content);
                        Thread.Sleep(100);

                        var verification = _fileReader.ReadFile(filename);
                        if (verification == content)
                        {
                            Console.WriteLine($"Successfully wrote to {filename}");
                        }
                        else
                        {
                            Console.WriteLine("Warning: File content verification failed");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error writing file: {ex.Message}");
                    }

                    break;
                case "delete":
                    if (args.Length < 1)
                        throw new ArgumentException("Missing argument for 'delete'. Usage: delete <filename>");
                    _fileWriter.DeleteFile(args[0]);
                    Console.WriteLine($"Deleted file {args[0]}");
                    break;
                case "move":
                    try
                    {
                        if (args.Length < 2)
                            throw new ArgumentException("Missing arguments. Usage: move <source> <destination>");

                        _fileMover.MoveFile(args[0], args[1]);
                        Console.WriteLine($"File moved successfully from {args[0]} to {args[1]}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during move operation: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Details: {ex.InnerException.Message}");
                        }
                    }

                    break;
                case "rename":
                    if (args.Length < 2)
                        throw new ArgumentException(
                            "Missing arguments for 'rename'. Usage: rename <sourcePath> <newName>");
                    _fileMover.RenameFile(args[0], args[1]);
                    Console.WriteLine($"Renamed file {args[0]} to {args[1]}");
                    break;
                case "rmdir":
                    if (args.Length < 1)
                        throw new ArgumentException("Missing argument for 'rmdir'. Usage: rmdir <directory>");
                    _fileSystemOperations.DeleteDirectory(args[0]);
                    Console.WriteLine($"Deleted directory {args[0]}");
                    break;
                case "forceremove":
                    if (args.Length < 1)
                        throw new ArgumentException(
                            "Missing argument for 'forceremove'. Usage: forceremove <path>");
                    _fileSystemOperations.ForceRemove(args[0]);
                    Console.WriteLine($"Force removed {args[0]}");
                    break;
                case "forcecopy":
                    if (args.Length < 2)
                        throw new ArgumentException(
                            "Missing arguments for 'forcecopy'. Usage: forcecopy <sourcePath> <destPath>");
                    _fileSystemOperations.ForceCopy(args[0], args[1]);
                    Console.WriteLine($"Force copied from {args[0]} to {args[1]}");
                    break;
                case "save":
                    if (args.Length < 2)
                        throw new ArgumentException("Missing arguments for 'save'. Usage: save <path> <content>");
                    var contentBytes = Encoding.ASCII.GetBytes(args[1]);
                    _fileWriter.SaveFile(args[0], contentBytes);
                    Console.WriteLine($"Saved file {args[0]}");
                    break;
                case "list":
                    if (args.Length < 1) args = new[] { CurrentPathManager.CurrentDirectory };
                    var files = _fileWriter.GetFileListing(args[0]);
                    Console.WriteLine($"Files in {args[0]}:");
                    foreach (var file in files)
                    {
                        Console.WriteLine(file);
                    }

                    break;
                case "cd":
                    if (args.Length < 1)
                        throw new ArgumentException("Missing argument for 'cd'. Usage: cd <directory>");
                    if (!CurrentPathManager.Set(args[0], out string errorMessage))
                    {
                        Console.WriteLine($"Error: {errorMessage}");
                    }
                    else
                    {
                        Console.WriteLine($"Current directory changed to {CurrentPathManager.CurrentDirectory}");
                    }

                    break;
                case "diskspace":
                    Console.WriteLine($"Free space: {FileUtils.GetFreeSpace()}");
                    Console.WriteLine($"Total capacity: {FileUtils.GetCapacity()}");
                    break;
                case "help":
                    if (args.Length > 0)
                        ShowCommandHelp(args[0]);
                    else
                        ShowHelp();
                    break;
                case "clearhistory":
                    _commandHistory.Clear();
                    Console.WriteLine("Command history cleared.");
                    break;
                case "alias":
                    if (args.Length < 2)
                        throw new ArgumentException("Missing argument for 'alias'. Usage: alias <alias> <command>");
                    _commandAliases[args[0]] = args[1];
                    Console.WriteLine($"Set alias {args[0]} for command {args[1]}");
                    break;
                case "batch":
                    if (args.Length < 1)
                        throw new ArgumentException("Missing argument for 'batch'. Usage: batch <filename>");
                    var commands = _fileReader.ReadFile(args[0]).Split('\n');
                    foreach (var cmd in commands)
                    {
                        ProcessCommand(cmd);
                    }

                    break;
                case "shutdown":
                    _shutdownCommand.Shutdown();
                    break;
                case "reboot":
                    _rebootCommand.Reboot();
                    break;
                case "pwd":
                    Console.WriteLine(CurrentPathManager.CurrentDirectory);
                    break;
                case "back":
                    if (!CurrentPathManager.Set("..", out string error))
                    {
                        Console.WriteLine($"Error: {error}");
                    }
                    else
                    {
                        Console.WriteLine($"Current directory changed to {CurrentPathManager.CurrentDirectory}");
                    }
                    break;
                case "netinit":
                    Console.WriteLine("Debug: Routing netinit command to network handler");
                    _networkHandler.HandleCommand("netinit", args);
                    break;

                case "connect":
                case "disconnect":
                case "send":
                case "ping":
                    _networkHandler.HandleCommand(command, args);
                    break;
                case "ftpstart":
                case "ftpstop":
                    _networkHandler.HandleCommand(command, args);
                    break;
                
                
                
                
                case "ps":
                    try
                    {
                        _processManager.ListProcesses();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error listing processes: {ex.Message}");
                    }
                    break;

                case "kill":
                    if (args.Length < 1)
                    {
                        Console.WriteLine("Usage: kill <pid>");
                        break;
                    }
                    if (int.TryParse(args[0], out int pid))
                    {
                        _processManager.BlockProcess(pid);
                    }
                    break;

                case "priority":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: priority <pid> <priority>");
                        break;
                    }
                    if (int.TryParse(args[0], out int p) && int.TryParse(args[1], out int prio))
                    {
                        _processManager.SetProcessPriority(p, prio);
                    }
                    break;

                case "pinfo":
                    if (args.Length < 1)
                    {
                        Console.WriteLine("Usage: pinfo <pid>");
                        break;
                    }
                    if (int.TryParse(args[0], out int procId))
                    {
                        var snapshot = _processManager.GetProcessSnapshot(procId);
                        if (snapshot != null)
                        {
                            Console.WriteLine(snapshot.ToString());
                        }
                    }
                    break;

                case "pstat":
                    var stats = _processManager.GetSchedulerStatistics();
                    Console.WriteLine($"Scheduler Statistics:");
                    Console.WriteLine($"Context Switches: {stats.ContextSwitches}");
                    Console.WriteLine($"Preemptions: {stats.Preemptions}");
                    Console.WriteLine($"Blocked Processes: {stats.BlockedProcesses}");
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine("Unknown command. Type 'help' to see available commands.");
                    Console.ResetColor();
                    break;
            }
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"Error: {e.Message}");
            Console.ResetColor();
        }
    }

    private void ShowHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("File Operations:");
        Console.WriteLine("  create <filename> - Creates a new empty file");
        Console.WriteLine("  read <filename> - Displays file contents");
        Console.WriteLine("  write <filename> <content> - Writes content to file");
        Console.WriteLine("  delete <filename> - Deletes a file");
        Console.WriteLine("  save <path> <content> - Saves binary content to file");
        Console.WriteLine("  list [path] - Lists files in directory");

        Console.WriteLine("\nDirectory Operations:");
        Console.WriteLine("  pwd - Print working directory");
        Console.WriteLine("  mkdir <dirname> - Creates new directory");
        Console.WriteLine("  rmdir <dirname> - Removes directory");
        Console.WriteLine("  cd <path> - Changes current directory");
        Console.WriteLine("  ls [path] - Lists directory contents");
        Console.WriteLine("  mvdir <source> <dest> - Moves directory");
        Console.WriteLine("  back - Go to parent directory");

        Console.WriteLine("\nFile/Directory Management:");
        Console.WriteLine("  move <source> <dest> - Moves file");
        Console.WriteLine("  rename <path> <newname> - Renames file/directory");
        Console.WriteLine("  forceremove <path> - Forces removal of file/directory");
        Console.WriteLine("  forcecopy <source> <dest> - Forces copy of file/directory");

        Console.WriteLine("\nSystem Commands:");
        Console.WriteLine("  diskspace - Shows disk space information");
        Console.WriteLine("  history - Shows command history");
        Console.WriteLine("  clearhistory - Clears command history");
        Console.WriteLine("  alias <name> <command> - Creates command alias");
        Console.WriteLine("  batch <filename> - Executes commands from file");
        Console.WriteLine("  shutdown - Shuts down the system");
        Console.WriteLine("  reboot - Restarts the system");
        
        Console.WriteLine("\nNetwork Commands:");
        Console.WriteLine("  netinit - Initialize network");
        Console.WriteLine("  connect <tcp|udp> <id> <ip> <port> - Connect to remote host");
        Console.WriteLine("  disconnect <id> - Disconnect from host");
        Console.WriteLine("  send <id> <message> - Send data to connected host");
        Console.WriteLine("  ping <ip> - Ping remote host");
        Console.WriteLine("  ftpstart - Start FTP server");
        Console.WriteLine("  ftpstop - Stop FTP server");
        
        Console.WriteLine("\nProcess Management:");
        Console.WriteLine("  ps - List all processes");
        Console.WriteLine("  kill <pid> - Terminate a process");
        Console.WriteLine("  priority <pid> <priority> - Set process priority");
        Console.WriteLine("  pinfo <pid> - Show process information");
        Console.WriteLine("  pstat - Show scheduler statistics");
    }

    private void ShowCommandHelp(string command)
    {
        var helpText = command switch
        {
            "create" => "Usage: create <filename> - Creates a new empty file",
            "read" => "Usage: read <filename> - Displays contents of file",
            "write" => "Usage: write <filename> <content> - Writes content to file",
            "delete" => "Usage: delete <filename> - Deletes specified file",
            "move" => "Usage: move <source> <destination> - Moves file to new location",
            "mkdir" => "Usage: mkdir <directory> - Creates new directory",
            "rmdir" => "Usage: rmdir <directory> - Removes directory",
            "back" => "Usage: back - Returns to parent directory",
            "ls" => "Usage: ls [path] - Lists contents of directory",
            "pwd" => "Usage: pwd - Prints current working directory",
            "mvdir" => "Usage: mvdir <source> <destination> - Moves directory to new location",
            "rename" => "Usage: rename <path> <newname> - Renames file or directory",
            "forceremove" => "Usage: forceremove <path> - Forces removal of file or directory",
            "forcecopy" => "Usage: forcecopy <source> <dest> - Forces copy of file or directory",
            "save" => "Usage: save <path> <content> - Saves binary content to file",
            "list" => "Usage: list [path] - Lists all files in directory",
            "cd" => "Usage: cd <directory> - Changes current working directory",
            "diskspace" => "Usage: diskspace - Shows disk space information",
            "history" => "Usage: history - Shows command history",
            "alias" => "Usage: alias <name> <command> - Creates command alias",
            "batch" => "Usage: batch <filename> - Executes commands from file",
            _ => $"No help available for '{command}'"
        };

        Console.WriteLine(helpText);
    }
}