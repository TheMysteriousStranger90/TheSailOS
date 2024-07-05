using System;
using System.IO;

namespace TheSailOS.FileSystem;

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

            var sourceFile = _fileTheSail._vfs.GetFile(sourcePath);
            var destinationFile = _fileTheSail._vfs.GetFile(destinationPath);

            if (sourceFile == null)
            {
                throw new FileNotFoundException($"Source file {sourcePath} not found");
            }

            if (destinationFile != null)
            {
                throw new Exception($"Destination file {destinationPath} already exists");
            }

            var content = _fileReader.ReadFile(sourcePath);

            _fileWriter.WriteFile(destinationPath, content);

            _fileTheSail._vfs.DeleteFile(sourceFile);
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