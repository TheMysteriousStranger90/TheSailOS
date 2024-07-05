using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.System.FileSystem.VFS;
using TheSailOS.FileSystem;
using Sys = Cosmos.System;

namespace TheSailOS
{
    public class Kernel : Sys.Kernel
    {
        public static string CurrentDirectory { get; private set; } = @"L:\";
        public static FileTheSail _fileTheSail;

        public static void SetCurrentDirectory(string path)
        {
            CurrentDirectory = path;
        }

        protected override void BeforeRun()
        {
            _fileTheSail = new FileTheSail();
            VFSManager.RegisterVFS(_fileTheSail._vfs);

            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            //Now only for testing purposes
            Console.Write("Input: ");
            var input = Console.ReadLine();
            var parts = input.Split(' ');
            if (parts.Length == 0) return;

            switch (parts[0].ToLower())
            {
                case "read":
                    if (parts.Length > 1)
                    {
                        FileReader fileReader = new FileReader(_fileTheSail);
                        try
                        {
                            string content = fileReader.ReadFile(parts[1]);
                            Console.WriteLine($"Content of {parts[1]}: {content}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error reading file: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Usage: read <filename>");
                    }

                    break;
                case "write":
                    if (parts.Length > 2)
                    {
                        FileWriter fileWriter = new FileWriter(_fileTheSail);
                        try
                        {
                            fileWriter.WriteFile(parts[1], string.Join(" ", parts.Skip(2)));
                            Console.WriteLine($"Written to {parts[1]}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error writing file: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Usage: write <filename> <content>");
                    }

                    break;
                case "cd":
                    if (parts.Length > 1)
                    {
                        SetCurrentDirectory(parts[1]);
                        Console.WriteLine($"Current directory set to {CurrentDirectory}");
                    }
                    else
                    {
                        Console.WriteLine("Usage: cd <path>");
                    }

                    break;
                default:
                    Console.WriteLine("Unknown command. Available commands: read, write, move, cd");
                    break;
            }
        }
    }
}