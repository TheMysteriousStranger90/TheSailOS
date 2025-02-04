using System;
using System.IO;
using System.Text;
using System.Threading;

namespace TheSailOS.FileSystemTheSail;

public class FileMover
{
    private FileTheSail _fileTheSail;
    private FileReader _fileReader;
    private FileWriter _fileWriter;

    public FileMover(FileTheSail fileTheSail, FileReader fileReader, FileWriter fileWriter)
    {
        this._fileTheSail = fileTheSail;
        this._fileReader = fileReader;
        this._fileWriter = fileWriter;
    }

    public void MoveFile(string sourcePath, string destinationPath)
    {
        try
        {
            sourcePath = Path.Combine(Kernel.CurrentDirectory, sourcePath);
            destinationPath = Path.Combine(Kernel.CurrentDirectory, destinationPath);

            Console.WriteLine($"Moving from: {sourcePath}");
            Console.WriteLine($"Moving to: {destinationPath}");

            var sourceFile = _fileTheSail._vfs.GetFile(sourcePath);
            if (sourceFile == null)
            {
                throw new FileNotFoundException($"Source file not found: {sourcePath}");
            }
            
            string destDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destDir))
            {
                var dir = _fileTheSail._vfs.GetDirectory(destDir);
                if (dir == null)
                {
                    _fileTheSail._vfs.CreateDirectory(destDir);
                    Thread.Sleep(50);
                }
            }
            
            string content = _fileReader.ReadFile(sourcePath);
            _fileWriter.WriteFile(destinationPath, content);
            _fileWriter.DeleteFile(sourcePath);

            Console.WriteLine("File moved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Move failed: {ex.Message}");
            throw;
        }
    }

    public void RenameFile(string sourcePath, string newName)
    {
        try
        {
            sourcePath = Path.Combine(Kernel.CurrentDirectory, sourcePath);
            Console.WriteLine($"Source path: {sourcePath}");

            var sourceFile = _fileTheSail._vfs.GetFile(sourcePath);
            if (sourceFile == null)
            {
                throw new FileNotFoundException($"Source file not found: {sourcePath}");
            }
            
            string sourceDir = Path.GetDirectoryName(sourcePath);
            string newPath = Path.Combine(sourceDir, newName);
            Console.WriteLine($"New path: {newPath}");

            if (_fileTheSail._vfs.GetFile(newPath) != null)
            {
                throw new IOException($"File already exists: {newPath}");
            }
            
            string content = _fileReader.ReadFile(sourcePath);
            _fileWriter.WriteFile(newPath, content);
            _fileWriter.DeleteFile(sourcePath);

            Console.WriteLine($"File renamed successfully to: {newPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Rename failed: {ex.Message}");
            throw;
        }
    }
}