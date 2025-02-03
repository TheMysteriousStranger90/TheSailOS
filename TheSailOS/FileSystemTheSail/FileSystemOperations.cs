using System;
using System.IO;
using Cosmos.System.FileSystem.Listing;

namespace TheSailOS.FileSystemTheSail;

public class FileSystemOperations
{
    private FileTheSail _fileTheSail;

    public FileSystemOperations(FileTheSail fileTheSail)
    {
        _fileTheSail = fileTheSail;
    }

    private string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be empty");
            
        var fullPath = Path.Combine(Kernel.CurrentDirectory, path);
        return fullPath.TrimEnd('\\') + '\\';
    }

    public void CreateDirectory(string path)
    {
        try
        {
            var fullPath = NormalizePath(path);
            
            if (_fileTheSail._vfs.GetDirectory(fullPath) != null)
                throw new IOException($"Directory already exists: {fullPath}");

            _fileTheSail._vfs.CreateDirectory(fullPath);
            Console.WriteLine($"Created directory: {fullPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating directory: {ex.Message}");
            throw;
        }
    }

    public void DeleteDirectory(string path, bool recursive = false)
    {
        try
        {
            var fullPath = NormalizePath(path);
            var directory = _fileTheSail._vfs.GetDirectory(fullPath);
            
            if (directory == null)
                throw new DirectoryNotFoundException($"Directory not found: {fullPath}");

            if (recursive)
            {
                var entries = _fileTheSail._vfs.GetDirectoryListing(directory);
                foreach (var entry in entries)
                {
                    var entryPath = Path.Combine(fullPath, entry.mName);
                    if (entry.mEntryType == DirectoryEntryTypeEnum.Directory)
                    {
                        DeleteDirectory(entryPath, true);
                    }
                    else
                    {
                        var file = _fileTheSail._vfs.GetFile(entryPath);
                        if (file != null)
                            _fileTheSail._vfs.DeleteFile(file);
                    }
                }
            }

            _fileTheSail._vfs.DeleteDirectory(directory);
            Console.WriteLine($"Deleted directory: {fullPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting directory: {ex.Message}");
            throw;
        }
    }

    public void MoveDirectory(string source, string destination)
    {
        try
        {
            var sourcePath = NormalizePath(source);
            var destPath = NormalizePath(destination);
            
            var sourceDir = _fileTheSail._vfs.GetDirectory(sourcePath);
            if (sourceDir == null)
                throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");

            if (_fileTheSail._vfs.GetDirectory(destPath) != null)
                throw new IOException($"Destination directory already exists: {destPath}");

            CopyDirectory(sourcePath, destPath);
            DeleteDirectory(sourcePath, true);
            Console.WriteLine($"Moved directory from {sourcePath} to {destPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error moving directory: {ex.Message}");
            throw;
        }
    }

    public void ListDirectory(string path)
    {
        try
        {
            var fullPath = NormalizePath(path);
            var directory = _fileTheSail._vfs.GetDirectory(fullPath);
            
            if (directory == null)
                throw new DirectoryNotFoundException($"Directory not found: {fullPath}");

            var entries = _fileTheSail._vfs.GetDirectoryListing(directory);
            foreach (var entry in entries)
            {
                var prefix = entry.mEntryType == DirectoryEntryTypeEnum.Directory ? "DIR" : "FILE";
                Console.WriteLine($"{prefix,-8}{entry.mName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing directory: {ex.Message}");
            throw;
        }
    }
    
    public void CopyDirectory(string sourceDir, string destDir)
    {
        sourceDir = Path.Combine(Kernel.CurrentDirectory, sourceDir);
        destDir = Path.Combine(Kernel.CurrentDirectory, destDir);
        CreateDirectory(destDir);

        var sourceDirectory = _fileTheSail._vfs.GetDirectory(sourceDir);
        if (sourceDirectory != null)
        {
            foreach (var entry in _fileTheSail._vfs.GetDirectoryListing(sourceDirectory))
            {
                string sourcePath = Path.Combine(sourceDir, entry.mName);
                string destPath = Path.Combine(destDir, entry.mName);

                if (entry.mEntryType == DirectoryEntryTypeEnum.Directory)
                {
                    CopyDirectory(sourcePath, destPath);
                }
                else
                {
                    var content = new FileReader(_fileTheSail).ReadFile(sourcePath);
                    new FileWriter(_fileTheSail).WriteFile(destPath, content);
                }
            }
        }
    }
    
    public void ForceRemove(string path)
    {
        path = Path.Combine(Kernel.CurrentDirectory, path);
        var file = _fileTheSail._vfs.GetFile(path);
        if (file != null)
        {
            _fileTheSail._vfs.DeleteFile(file);
        }
        else
        {
            var directory = _fileTheSail._vfs.GetDirectory(path);
            if (directory != null)
            {
                DeleteDirectory(path, true);
            }
        }
    }
    
    public void ForceCopy(string sourcePath, string destPath)
    {
        sourcePath = Path.Combine(Kernel.CurrentDirectory, sourcePath);
        destPath = Path.Combine(Kernel.CurrentDirectory, destPath);
        if (_fileTheSail._vfs.GetFile(sourcePath) != null)
        {
            var content = new FileReader(_fileTheSail).ReadFile(sourcePath);
            new FileWriter(_fileTheSail).WriteFile(destPath, content);
        }
        else if (_fileTheSail._vfs.GetDirectory(sourcePath) != null)
        {
            CopyDirectory(sourcePath, destPath);
        }
    }
}