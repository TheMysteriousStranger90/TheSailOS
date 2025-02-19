using System;
using System.Collections.Generic;
using System.IO;
using TheSailOSProject.Styles;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Applications
{
    public class TextEditor
    {
        private static bool _isModified;
        private static IFileManager _fileManager;

        public static void Run(string filePath, IFileManager fileManager)
        {
            Console.Clear();
            ConsoleManager.WriteLineColored("Simple Text Editor", ConsoleStyle.Colors.Primary);

            List<string> lines = new List<string>();
            _isModified = false;
            _fileManager = fileManager;

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try
                {
                    lines.AddRange(File.ReadAllLines(filePath));
                    ConsoleManager.WriteLineColored($"Loaded file: {filePath}", ConsoleStyle.Colors.Success);
                }
                catch (Exception ex)
                {
                    ConsoleManager.WriteLineColored($"Error loading file: {ex.Message}", ConsoleStyle.Colors.Error);
                }
            }
            else if (!string.IsNullOrEmpty(filePath))
            {
                ConsoleManager.WriteLineColored($"Creating new file: {filePath}", ConsoleStyle.Colors.Warning);
            }

            while (true)
            {
                ConsoleManager.WriteColored(">", ConsoleStyle.Colors.Primary);
                string input = Console.ReadLine();

                if (input.ToLower() == "exit")
                {
                    if (_isModified)
                    {
                        ConsoleManager.WriteColored("File is modified. Save before exit? (y/n): ", ConsoleStyle.Colors.Warning);
                        string saveChoice = Console.ReadLine().ToLower();
                        if (saveChoice == "y")
                        {
                            SaveFile(filePath, lines);
                        }
                    }
                    break;
                }
                else if (input.ToLower() == "save")
                {
                    if (string.IsNullOrEmpty(filePath))
                    {
                        ConsoleManager.WriteLineColored("Enter file path:", ConsoleStyle.Colors.Primary);
                        filePath = Console.ReadLine();
                    }
                    SaveFile(filePath, lines);
                    _isModified = false;
                }
                else if (input.ToLower() == "list")
                {
                    ListLines(lines);
                }
                else if (input.StartsWith("insert "))
                {
                    InsertLine(lines, input.Substring(7));
                    _isModified = true;
                }
                else if (input.StartsWith("delete "))
                {
                    DeleteLine(lines, input.Substring(7));
                    _isModified = true;
                }
                else if (input.StartsWith("replace "))
                {
                    ReplaceLine(lines, input.Substring(8));
                    _isModified = true;
                }
                else
                {
                    ConsoleManager.WriteLineColored("Available commands: exit, save, list, insert <text>, delete <index>, replace <index> <text>", ConsoleStyle.Colors.Accent);
                }

                DisplayFileStatus(_isModified);
            }

            ConsoleManager.WriteLineColored("Text editor closed.", ConsoleStyle.Colors.Primary);
            Console.ReadKey();
        }

        private static void SaveFile(string filePath, List<string> lines)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                ConsoleManager.WriteLineColored("Error: No file path specified.", ConsoleStyle.Colors.Error);
                return;
            }

            try
            {
                // Use the WriteFile method from TheSailFileSystem
                _fileManager.WriteFile(filePath, lines);

                ConsoleManager.WriteLineColored($"File saved to: {filePath}", ConsoleStyle.Colors.Success);
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error saving file: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }

        private static void ListLines(List<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                ConsoleManager.WriteLineColored($"{i + 1}: {lines[i]}", ConsoleStyle.Colors.Accent);
            }
        }

        private static void InsertLine(List<string> lines, string text)
        {
            lines.Add(text);
            ConsoleManager.WriteLineColored($"Inserted: {text}", ConsoleStyle.Colors.Success);
        }

        private static void DeleteLine(List<string> lines, string indexStr)
        {
            if (int.TryParse(indexStr, out int index) && index >= 1 && index <= lines.Count)
            {
                lines.RemoveAt(index - 1);
                ConsoleManager.WriteLineColored($"Deleted line {index}", ConsoleStyle.Colors.Success);
            }
            else
            {
                ConsoleManager.WriteLineColored("Invalid line number.", ConsoleStyle.Colors.Error);
            }
        }

        private static void ReplaceLine(List<string> lines, string input)
        {
            string[] parts = input.Split(' ');
            if (parts.Length < 2 || !int.TryParse(parts[0], out int index) || index < 1 || index > lines.Count)
            {
                ConsoleManager.WriteLineColored("Invalid replace format. Usage: replace <index> <text>", ConsoleStyle.Colors.Error);
                return;
            }

            string text = string.Join(" ", parts, 1, parts.Length - 1);
            lines[index - 1] = text;
            ConsoleManager.WriteLineColored($"Replaced line {index} with: {text}", ConsoleStyle.Colors.Success);
        }

        private static void DisplayFileStatus(bool isModified)
        {
            if (isModified)
            {
                ConsoleManager.WriteLineColored("File status: Modified", ConsoleStyle.Colors.Warning);
            }
            else
            {
                ConsoleManager.WriteLineColored("File status: Saved", ConsoleStyle.Colors.Success);
            }
        }
    }
}