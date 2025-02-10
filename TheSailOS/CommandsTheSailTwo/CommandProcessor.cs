using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheSailOS.Exceptions;

namespace TheSailOS.CommandsTheSailTwo;
public class CommandProcessor
{
    private List<string> _commandHistory = new List<string>();
    private List<string> _availableCommands = new List<string>
    {
        "ls", "dir", "cd", "mkdir", "rmdir", "create", "delete", "read", "write", "copy", "move", "rename", "info", "history", "clear", "help", "alias", "reboot", "shutdown"
    };
    private Dictionary<string, string> _commandAliases = new Dictionary<string, string>();
    private AliasManager _aliasManager;
    private CommandHistoryManager _historyManager;
    private FileSystem _fileSystem;
    private string _currentDirectory;
    private string _rootDirectory = @"0:\"; // Configurable root directory

    public CommandProcessor(FileSystem fileSystem, string rootDirectory = @"0:\")
    {
        _aliasManager = new AliasManager(_availableCommands);
        _historyManager = new CommandHistoryManager();
        _fileSystem = fileSystem;
        _rootDirectory = rootDirectory;
        _currentDirectory = _rootDirectory;
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
                case "dir":
                    ListFiles(args);
                    break;
                case "cd":
                    ChangeDirectory(args);
                    break;
                case "mkdir":
                    CreateDirectory(args);
                    break;
                case "rmdir":
                    DeleteDirectory(args);
                    break;
                case "create":
                    CreateFile(args);
                    break;
                case "delete":
                    DeleteFile(args);
                    break;
                case "read":
                    ReadFile(args);
                    break;
                case "write":
                    WriteFile(args);
                    break;
                case "copy":
                    CopyFile(args);
                    break;
                case "move":
                    MoveFile(args);
                    break;
                case "rename":
                    Rename(args);
                    break;
                case "history":
                    _historyManager.ShowHistory();
                    break;
                case "clear":
                    _historyManager.Clear();
                    break;
                case "help":
                    if (args.Length > 0)
                    {
                        ShowCommandHelp(args[0]);
                    }
                    else
                    {
                        ShowHelp();
                    }
                    break;
                case "alias":
                    CreateAlias(args);
                    break;
                case "reboot":
                    Cosmos.System.Power.Reboot();
                    break;
                case "shutdown":
                    Cosmos.System.Power.Shutdown();
                    break;
                default:
                    Console.WriteLine($"Error: Unknown command '{command}'.");
                    break;
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Argument Error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access Denied: {ex.Message}");
        }
        catch (FileSystemException ex)
        {
            Console.WriteLine($"File System Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: {ex.Message}");
            LogException(ex);
        }
    }

    private void ListFiles(string[] args)
    {
        string path = args.Length > 0 ? CombinePath(_currentDirectory, args[0]) : _currentDirectory;
        try
        {
            ValidatePath(path);
            string[] files = _fileSystem.ListFiles(path);
            if (files != null)
            {
                foreach (string file in files)
                {
                    Console.WriteLine(Path.GetFileName(file)); // Show only the file name
                }
            }

            string[] directories = _fileSystem.ListDirectories(path);
            if (directories != null)
            {
                foreach (string dir in directories)
                {
                    Console.WriteLine(Path.GetFileName(dir));
                }
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error listing files: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing files: {ex.Message}");
        }
    }

    private void ChangeDirectory(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: cd <path>");
            return;
        }

        string path = CombinePath(_currentDirectory, args[0]);

        try
        {
            ValidatePath(path);

            if (args[0] == "..")
            {
                if (_currentDirectory != _rootDirectory)
                {
                    // More robust ".." handling
                    DirectoryInfo dirInfo = new DirectoryInfo(_currentDirectory);
                    if (dirInfo.Parent != null)
                    {
                        _currentDirectory = dirInfo.Parent.FullName;
                    }
                    else
                    {
                        _currentDirectory = _rootDirectory; // Already at root
                    }
                }
                Console.WriteLine($"Current directory: {_currentDirectory}");
                return;
            }

            if (Directory.Exists(path))
            {
                _currentDirectory = path;
                Console.WriteLine($"Current directory: {_currentDirectory}");
            }
            else
            {
                Console.WriteLine($"Directory not found: {path}");
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error changing directory: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error changing directory: {ex.Message}");
        }
    }

    private void CreateDirectory(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: mkdir <path>");
            return;
        }

        string path = CombinePath(_currentDirectory, args[0]);
        try
        {
            ValidatePath(path);
            _fileSystem.CreateDirectory(path);
            Console.WriteLine($"Directory created: {path}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error creating directory: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating directory: {ex.Message}");
        }
    }

    private void DeleteDirectory(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: rmdir <path>");
            return;
        }

        string path = CombinePath(_currentDirectory, args[0]);
        try
        {
            ValidatePath(path);
            _fileSystem.DeleteDirectory(path);
            Console.WriteLine($"Directory deleted: {path}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error deleting directory: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting directory: {ex.Message}");
        }
    }

    private void CreateFile(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: create <path>");
            return;
        }

        string path = CombinePath(_currentDirectory, args[0]);
        try
        {
            ValidatePath(path);
            _fileSystem.CreateFile(path);
            Console.WriteLine($"File created: {path}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error creating file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating file: {ex.Message}");
        }
    }

    private void DeleteFile(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: delete <path>");
            return;
        }

        string path = CombinePath(_currentDirectory, args[0]);
        try
        {
            ValidatePath(path);
            _fileSystem.DeleteFile(path);
            Console.WriteLine($"File deleted: {path}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
        }
    }

    private void ReadFile(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: read <path>");
            return;
        }

        string path = CombinePath(_currentDirectory, args[0]);
        try
        {
            ValidatePath(path);
            string content = _fileSystem.ReadFile(path);
            if (content != null)
            {
                Console.WriteLine($"Content of {path}:\n{content}");
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
        }
    }

    private void WriteFile(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: write <path> <content>");
            return;
        }

        string path = CombinePath(_currentDirectory, args[0]);
        string content = string.Join(" ", args.Skip(1));
        try
        {
            ValidatePath(path);
            _fileSystem.WriteFile(path, content);
            Console.WriteLine($"Wrote to file: {path}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error writing to file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to file: {ex.Message}");
        }
    }

    private void CopyFile(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: copy <source> <destination>");
            return;
        }

        string source = CombinePath(_currentDirectory, args[0]);
        string destination = CombinePath(_currentDirectory, args[1]);
        try
        {
            ValidatePath(source);
            ValidatePath(destination);
            _fileSystem.CopyFile(source, destination);
            Console.WriteLine($"Copied {source} to {destination}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error copying file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copying file: {ex.Message}");
        }
    }

    private void MoveFile(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: move <source> <destination>");
            return;
        }

        string sourcePath = CombinePath(_currentDirectory, args[0]);
        string destinationPath = CombinePath(_currentDirectory, args[1]);

        try
        {
            ValidatePath(sourcePath);
            ValidatePath(destinationPath);
            _fileSystem.MoveFile(sourcePath, destinationPath);
            Console.WriteLine($"Moved {sourcePath} to {destinationPath}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error moving file: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error moving file: {ex.Message}");
        }
    }

    private void Rename(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: rename <oldPath> <newName>");
            return;
        }

        string sourcePath = CombinePath(_currentDirectory, args[0]);
        string newName = args[1];

        try
        {
            ValidatePath(sourcePath);
            _fileSystem.RenameFile(sourcePath, newName);
            Console.WriteLine($"Renamed {sourcePath} to {newName}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error renaming: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error renaming: {ex.Message}");
        }
    }
    
    private void CreateAlias(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: alias <new_alias> <command>");
            return;
        }

        string alias = args[0];
        string command = args[1];

        try
        {
            _aliasManager.CreateAlias(alias, command);
            _commandAliases[alias] = command;
            Console.WriteLine($"Alias '{alias}' created for command '{command}'.");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error creating alias: {ex.Message}");
        }
    }

    private void ShowCommandHelp(string command)
    {
        var helpText = command switch
        {
            "ls" or "dir" => "Lists files and directories in the current directory or specified path.\nUsage: ls [path]",
            "cd" => "Changes the current directory.\nUsage: cd <path>",
            "mkdir" => "Creates a new directory.\nUsage: mkdir <path>",
            "rmdir" => "Deletes a directory.\nUsage: rmdir <path>",
            "create" => "Creates a new file.\nUsage: create <path>",
            "delete" => "Deletes a file.\nUsage: delete <path>",
            "read" => "Reads and displays the content of a file.\nUsage: read <path>",
            "write" => "Writes content to a file.\nUsage: write <path> <content>",
            "copy" => "Copies a file from source to destination.\nUsage: copy <source> <destination>",
            "move" => "Moves a file from source to destination.\nUsage: move <source> <destination>",
            "rename" => "Renames a file.\nUsage: rename <oldPath> <newName>",
            "info" => "Displays information about a file or directory.\nUsage: info <path>",
            "history" => "Displays the command history.\nUsage: history",
            "clear" => "Clears the command history.\nUsage: clear",
            "help" => "Displays help information for a command or lists available commands.\nUsage: help [command]",
            "alias" => "Creates an alias for a command.\nUsage: alias <new_alias> <command>",
            "reboot" => "Reboots the system.\nUsage: reboot",
            "shutdown" => "Shuts down the system.\nUsage: shutdown",
            _ => $"No help available for command '{command}'.",
        };

        Console.WriteLine(helpText);
    }

    private void ShowHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("File Operations:");
        Console.WriteLine("  ls/dir   - List files and directories");
        Console.WriteLine("  cd       - Change directory");
        Console.WriteLine("  mkdir    - Create directory");
        Console.WriteLine("  rmdir    - Remove directory");
        Console.WriteLine("  create   - Create file");
        Console.WriteLine("  delete   - Delete file");
        Console.WriteLine("  read     - Read file content");
        Console.WriteLine("  write    - Write to file");
        Console.WriteLine("  copy     - Copy file");
        Console.WriteLine("  move     - Move file");
        Console.WriteLine("  rename   - Rename file");
        Console.WriteLine("  info     - Get file/directory information");
        Console.WriteLine("System Operations:");
        Console.WriteLine("  history  - Show command history");
        Console.WriteLine("  clear    - Clear command history");
        Console.WriteLine("  help     - Show help");
        Console.WriteLine("  alias    - Create command alias");
        Console.WriteLine("  reboot   - Reboot the system");
        Console.WriteLine("  shutdown - Shutdown the system");
        Console.WriteLine("Type 'help <command>' for more information on a specific command.");
    }
    
    private string CombinePath(string basePath, string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
        {
            return basePath;
        }

        if (Path.IsPathRooted(relativePath))
        {
            return relativePath;
        }

        char lastChar = basePath[basePath.Length - 1];
        if (lastChar != '\\')
        {
            return basePath + "\\" + relativePath;
        }
        else
        {
            return basePath + relativePath;
        }
    }

    private void ValidatePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path cannot be null or empty.");
        }

        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            throw new ArgumentException("Path contains invalid characters.");
        }

        if (!path.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Path is outside the allowed file system root.");
        }
    }

    private void LogException(Exception ex)
    {
        // Implement logging to a file or other persistent storage
        // This is a placeholder
        Console.WriteLine($"[ERROR - LOGGED] {DateTime.Now}: {ex.ToString()}");
    }
}