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
/*
    public void MoveFile(string sourcePath, string destinationPath)
    {
        try
        {
            sourcePath = Path.Combine(Kernel.CurrentDirectory, sourcePath);
            destinationPath = Path.Combine(Kernel.CurrentDirectory, destinationPath);

            var sourceFile = _fileTheSail._vfs.GetFile(sourcePath);
            if (sourceFile == null)
            {
                throw new FileNotFoundException($"Source file {sourcePath} not found");
            }

            string destDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destDir) && _fileTheSail._vfs.GetDirectory(destDir) == null)
            {
                throw new DirectoryNotFoundException($"Destination directory {destDir} not found");
            }

            if (_fileTheSail._vfs.GetFile(destinationPath) != null)
            {
                throw new IOException($"Destination file {destinationPath} already exists");
            }

            string content = _fileReader.ReadFile(sourcePath);

            _fileWriter.WriteFile(destinationPath, content);

            _fileWriter.DeleteFile(sourcePath);

            Thread.Sleep(100);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to move file: {ex.Message}", ex);
        }
    }
*/

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

            var sourceFile = _fileTheSail._vfs.GetFile(sourcePath);
            if (sourceFile == null)
            {
                throw new FileNotFoundException($"Source file {sourcePath} not found");
            }

            var newPath = Path.Combine(Path.GetDirectoryName(sourcePath), newName);
            newPath = Path.Combine(Kernel.CurrentDirectory, newPath);

            MoveFile(sourcePath, newPath);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}