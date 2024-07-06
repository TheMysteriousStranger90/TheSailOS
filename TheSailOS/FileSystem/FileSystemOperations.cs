using System;
using System.IO;
using Cosmos.System.FileSystem.Listing;

namespace TheSailOS.FileSystem;

public class FileSystemOperations
{
    private FileTheSail _fileTheSail;

    public FileSystemOperations(FileTheSail fileTheSail)
    {
        _fileTheSail = fileTheSail;
    }

    public void CreateDirectory(string path)
    {
        if (string.IsNullOrEmpty(Kernel.CurrentDirectory) || string.IsNullOrEmpty(path))
        {
            Console.WriteLine($"Invalid path components: Kernel.CurrentDirectory={Kernel.CurrentDirectory}, path={path}");
            return;
        }
        
        var fullPath = Path.Combine(Kernel.CurrentDirectory, path);
        if (_fileTheSail._vfs == null)
        {
            Console.WriteLine("Error: _vfs is null. CosmosVFS instance is not initialized.");
            return;
        }
        
        Console.WriteLine($"Full path for directory creation: {fullPath}");
        _fileTheSail._vfs.CreateDirectory(fullPath);
    }

    public void DeleteDirectory(string path, bool recursive = false)
    {
        path = Path.Combine(Kernel.CurrentDirectory, path);
        var directory = _fileTheSail._vfs.GetDirectory(path);
        if (directory != null)
        {
            if (recursive)
            {
                var entries = _fileTheSail._vfs.GetDirectoryListing(directory);
                foreach (var entry in entries)
                {
                    string fullPath = Path.Combine(path, entry.mName);
                    if (entry.mEntryType == DirectoryEntryTypeEnum.Directory)
                    {
                        DeleteDirectory(fullPath, true);
                    }
                    else
                    {
                        var fileToDelete = _fileTheSail._vfs.GetFile(fullPath);
                        if (fileToDelete != null)
                        {
                            _fileTheSail._vfs.DeleteFile(fileToDelete);
                        }
                    }
                }
            }

            _fileTheSail._vfs.DeleteDirectory(directory);
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