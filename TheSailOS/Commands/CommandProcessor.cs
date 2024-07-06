using System;
using System.Collections.Generic;
using System.Linq;
using System.CommandLine;
using System.Text;
using TheSailOS.FileSystem;

namespace TheSailOS.Commands;

public class CommandProcessor
{
    private readonly FileReader _fileReader;
    private readonly FileWriter _fileWriter;
    private readonly FileMover _fileMover;
    private readonly FileSystemOperations _fileSystemOperations;

    private List<string> _commandHistory = new List<string>();

    private List<string> _availableCommands = new List<string>
    {
        "create", "read", "write", "delete", "move", "mkdir", "rmdir", "ls", "mvdir", "help", "history", "alias",
        "batch", "rename", "forceremove", "forcecopy", "save", "list", "cd", "diskspace"
    };

    private Dictionary<string, string> _commandAliases = new Dictionary<string, string>();

    public CommandProcessor(FileReader fileReader, FileWriter fileWriter, FileMover fileMover,
        FileSystemOperations fileSystemOperations)
    {
        this._fileReader = fileReader;
        this._fileWriter = fileWriter;
        this._fileMover = fileMover;
        this._fileSystemOperations = fileSystemOperations;
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

        try
        {
            switch (command)
            {
                case "mkdir":
                    if (args.Length < 1)
                        throw new ArgumentException("Missing argument for 'mkdir'. Usage: mkdir <directory>");
                    _fileSystemOperations.CreateDirectory(args[0]);
                    Console.WriteLine($"Created directory {args[0]}");
                    break;
                case "create":
                    if (args.Length < 1)
                        throw new ArgumentException("Missing argument for 'create'. Usage: create <filename>");
                    _fileWriter.WriteFile(args[0], "");
                    Console.WriteLine($"Created file {args[0]}");
                    break;
                case "read":
                    if (args.Length < 1)
                        throw new ArgumentException("Missing argument for 'read'. Usage: read <filename>");
                    var content = _fileReader.ReadFile(args[0]);
                    Console.WriteLine(content);
                    break;
                case "write":
                    if (args.Length < 2)
                        throw new ArgumentException(
                            "Missing argument for 'write'. Usage: write <filename> <content>");
                    _fileWriter.WriteFile(args[0], args[1]);
                    Console.WriteLine($"Wrote to file {args[0]}");
                    break;
                case "delete":
                    if (args.Length < 1)
                        throw new ArgumentException("Missing argument for 'delete'. Usage: delete <filename>");
                    _fileWriter.DeleteFile(args[0]);
                    Console.WriteLine($"Deleted file {args[0]}");
                    break;
                case "move":
                    if (args.Length < 2)
                        throw new ArgumentException(
                            "Missing argument for 'move'. Usage: move <source> <destination>");
                    _fileMover.MoveFile(args[0], args[1]);
                    Console.WriteLine($"Moved file from {args[0]} to {args[1]}");
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
                    if (!CurrentPathManager.Set(args[0], out string error))
                    {
                        Console.WriteLine($"Error: {error}");
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
                    Console.WriteLine("Available commands:");
                    Console.WriteLine("create <filename> - Creates a new file");
                    Console.WriteLine("read <filename> - Reads a file");
                    Console.WriteLine("write <filename> <content> - Writes to a file");
                    Console.WriteLine("delete <filename> - Deletes a file");
                    Console.WriteLine("move <source> <destination> - Moves a file");
                    break;
                case "history":
                    foreach (var item in _commandHistory)
                    {
                        Console.WriteLine(item);
                    }

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

    public void InteractiveMode()
    {
        var rootCommand = new RootCommand();

        foreach (var command in _availableCommands)
        {
            rootCommand.AddCommand(new Command(command));
        }

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (input == "exit")
            {
                break;
            }

            try
            {
                var parseResult = rootCommand.Parse(input ?? throw new InvalidOperationException());
                var command = parseResult.CommandResult.Command.Name;

                ProcessCommand(command);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    public void SetupAutocomplete()
    {
        Console.WriteLine("Autocomplete setup. When typing a command, press Tab to see available options.");
    }
}