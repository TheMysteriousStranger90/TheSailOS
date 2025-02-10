using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using TheSailOS.Exceptions;
using System.Threading;

public class FileSystem
{
    private CosmosVFS _vfs;
    private Dictionary<string, byte[]> _fileCache = new Dictionary<string, byte[]> ();
    private const string SystemDirectory = @"0:\System";

    public void Initialize()
    {
        try
        {
            _vfs = new CosmosVFS();
            VFSManager.RegisterVFS(_vfs);
            Console.WriteLine("[INFO] Virtual filesystem initialized");
            
            if (!Directory.Exists(SystemDirectory))
            {
                Directory.CreateDirectory(SystemDirectory);
                Console.WriteLine($"[INFO] Created system directory: {SystemDirectory}");
            }

            foreach (var disk in VFSManager.GetDisks())
            {
                Console.WriteLine($"[INFO] Mounting disk {VFSManager.GetDisks().IndexOf(disk)}...");
                disk.Mount();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to initialize filesystem: {ex.Message}");
            throw new FileSystemException("Failed to initialize filesystem", ex);
        }
    }

    public bool CreateFile(string path)
    {
        if (IsSystemPath(path))
        {
            throw new UnauthorizedAccessException("Cannot create files in system directory.");
        }

        try
        {
            VFSManager.CreateFile(path);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to create file: {ex.Message}");
            throw new FileSystemException($"Failed to create file: {path}", ex);
        }
    }

    public bool DeleteFile(string path)
    {
        if (IsSystemPath(path))
        {
            throw new UnauthorizedAccessException("Cannot delete files in system directory.");
        }

        try
        {
            VFSManager.DeleteFile(path); // Use VFSManager to delete files
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to delete file: {ex.Message}");
            throw new FileSystemException($"Failed to delete file: {path}", ex);
        }
    }

    public string ReadFile(string path)
    {
        try
        {
            if (_fileCache.ContainsKey(path))
            {
                return System.Text.Encoding.UTF8.GetString(_fileCache[path]);
            }

            string content = File.ReadAllText(path);

            _fileCache[path] = System.Text.Encoding.UTF8.GetBytes(content);

            return content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to read file: {ex.Message}");
            throw new FileSystemException($"Failed to read file: {path}", ex);
        }
    }

    public bool WriteFile(string path, string content)
    {
        if (IsSystemPath(path))
        {
            throw new UnauthorizedAccessException("Cannot write to files in system directory.");
        }

        try
        {
            File.WriteAllText(path, content);

            _fileCache[path] = System.Text.Encoding.UTF8.GetBytes(content);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to write file: {ex.Message}");
            throw new FileSystemException($"Failed to write file: {path}", ex);
        }
    }

    public bool CreateDirectory(string path)
    {
        if (IsSystemPath(path))
        {
            throw new UnauthorizedAccessException("Cannot create directories in system directory.");
        }

        try
        {
            VFSManager.CreateDirectory(path);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to create directory: {ex.Message}");
            throw new FileSystemException($"Failed to create directory: {path}", ex);
        }
    }

    public bool DeleteDirectory(string path)
    {
        if (IsSystemPath(path))
        {
            throw new UnauthorizedAccessException("Cannot delete directories in system directory.");
        }

        try
        {
            VFSManager.DeleteDirectory(path, true);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to delete directory: {ex.Message}");
            throw new FileSystemException($"Failed to delete directory: {path}", ex);
        }
    }

    public string[] ListFiles(string path)
    {
        try
        {
            return Directory.GetFiles(path);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to list files: {ex.Message}");
            throw new FileSystemException($"Failed to list files in: {path}", ex);
        }
    }

    public string[] ListDirectories(string path)
    {
        try
        {
            return Directory.GetDirectories(path);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to list directories: {ex.Message}");
            throw new FileSystemException($"Failed to list directories in: {path}", ex);
        }
    }

    public long GetAvailableFreeSpace(string drive)
    {
        try
        {
            return _vfs.GetAvailableFreeSpace(drive);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to get available free space: {ex.Message}");
            throw new FileSystemException($"Failed to get available free space on: {drive}", ex);
        }
    }

    public string GetFileSystemType(string drive)
    {
        try
        {
            return _vfs.GetFileSystemType(drive);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to get filesystem type: {ex.Message}");
            throw new FileSystemException($"Failed to get filesystem type on: {drive}", ex);
        }
    }

    public void CopyFile(string source, string destination)
    {
        if (IsSystemPath(destination))
        {
            throw new UnauthorizedAccessException("Cannot copy files to system directory.");
        }

        try
        {
            File.Copy(source, destination);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to copy file: {ex.Message}");
            throw new FileSystemException($"Failed to copy file from {source} to {destination}", ex);
        }
    }

    public byte[] ReadFileBytes(string path)
    {
        try
        {
            if (_fileCache.ContainsKey(path))
            {
                return _fileCache[path];
            }
            else
            {
                byte[] data = File.ReadAllBytes(path);
                _fileCache[path] = data;
                return data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to read file bytes: {ex.Message}");
            throw new FileSystemException($"Failed to read file bytes from: {path}", ex);
        }
    }

    public bool MoveFile(string sourcePath, string destinationPath)
    {
        if (IsSystemPath(sourcePath) || IsSystemPath(destinationPath))
        {
            throw new UnauthorizedAccessException("Cannot move files to or from system directory.");
        }

        try
        {
            //sourcePath = Path.Combine(Kernel.CurrentDirectory, sourcePath); //Kernel doesnt exist
            //destinationPath = Path.Combine(Kernel.CurrentDirectory, destinationPath);

            Console.WriteLine($"Moving from: {sourcePath}");
            Console.WriteLine($"Moving to: {destinationPath}");

            var sourceFile = _vfs.GetFile(sourcePath);
            if (sourceFile == null)
            {
                throw new FileNotFoundException($"Source file not found: {sourcePath}");
            }
            
            string destDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destDir))
            {
                if (!Directory.Exists(destDir))
                {
                    VFSManager.CreateDirectory(destDir);
                    Thread.Sleep(50);
                }
            }
            
            string content = ReadFile(sourcePath);
            WriteFile(destinationPath, content);
            DeleteFile(sourcePath);

            Console.WriteLine("File moved successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Move failed: {ex.Message}");
            throw new FileSystemException($"Failed to move file from {sourcePath} to {destinationPath}", ex);
        }
    }

    public bool RenameFile(string sourcePath, string newName)
    {
        if (IsSystemPath(sourcePath))
        {
            throw new UnauthorizedAccessException("Cannot rename files in system directory.");
        }

        try
        {
            //sourcePath = Path.Combine(Kernel.CurrentDirectory, sourcePath); //Kernel doesnt exist
            Console.WriteLine($"Source path: {sourcePath}");

            var sourceFile = _vfs.GetFile(sourcePath);
            if (sourceFile == null)
            {
                throw new FileNotFoundException($"Source file not found: {sourcePath}");
            }
            
            string sourceDir = Path.GetDirectoryName(sourcePath);
            string newPath = Path.Combine(sourceDir, newName);
            Console.WriteLine($"New path: {newPath}");

            if (_vfs.GetFile(newPath) != null)
            {
                throw new IOException($"File already exists: {newPath}");
            }
            
            string content = ReadFile(sourcePath);
            WriteFile(newPath, content);
            DeleteFile(sourcePath);

            Console.WriteLine($"File renamed successfully to: {newPath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Rename failed: {ex.Message}");
            throw new FileSystemException($"Failed to rename file: {sourcePath} to {newName}", ex);
        }
    }

    public bool RenameDirectory(string oldPath, string newPath)
    {
        if (IsSystemPath(oldPath) || IsSystemPath(newPath))
        {
            throw new UnauthorizedAccessException("Cannot rename directories in system directory.");
        }

        try
        {
            Directory.Move(oldPath, newPath);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to rename directory: {ex.Message}");
            throw new FileSystemException($"Failed to rename directory from {oldPath} to {newPath}", ex);
        }
    }

    public FileAttributes GetFileAttributes(string path)
    {
        try
        {
            return File.GetAttributes(path);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to get file attributes: {ex.Message}");
            throw new FileSystemException($"Failed to get file attributes for: {path}", ex);
        }
    }

    public FileInfo GetFileInfo(string path)
    {
        try
        {
            return new FileInfo(path);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to get file info: {ex.Message}");
            throw new FileSystemException($"Failed to get file info for: {path}", ex);
        }
    }

    private bool IsSystemPath(string path)
    {
        return path.StartsWith(SystemDirectory, StringComparison.OrdinalIgnoreCase);
    }
}