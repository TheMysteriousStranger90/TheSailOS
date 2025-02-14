using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;
using TheSailOSProject.Exceptions;
using TheSailOSProject.Styles;

namespace TheSailOSProject.FileSystem;

public class TheSailFileSystem : CosmosVFS, IFileManager, IDirectoryManager, ICacheManager, IVFSManager, IDiskManager
{
    private CosmosVFS _vfs;
    private Dictionary<string, byte[]> _fileCache;
    private const string SystemDirectory = @"0:\System";

    public void Initialize()
    {
        try
        {
            _vfs = new CosmosVFS();
            VFSManager.RegisterVFS(_vfs);

            _fileCache = new Dictionary<string, byte[]>();
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

    // IFileManager implementation
    public bool CreateFile(string path)
    {
        EnsureNotSystemPath(path);

        try
        {
            VFSManager.CreateFile(path);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            throw new FileSystemException($"Operation failed: {ex.Message}", ex);
        }
    }

    public bool DeleteFile(string path)
    {
        EnsureNotSystemPath(path);

        try
        {
            VFSManager.DeleteFile(path);
            _fileCache.Remove(path);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            throw new FileSystemException($"Operation failed: {ex.Message}", ex);
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
            Console.WriteLine($"[ERROR] {ex.Message}");
            throw new FileSystemException($"Operation failed: {ex.Message}", ex);
        }
    }

    public bool WriteFile(string path, string content)
    {
        EnsureNotSystemPath(path);

        try
        {
            File.WriteAllText(path, content);
            _fileCache[path] = System.Text.Encoding.UTF8.GetBytes(content);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            throw new FileSystemException($"Operation failed: {ex.Message}", ex);
        }
    }

    public bool RenameFile(string path, string newName)
    {
        EnsureNotSystemPath(path);

        try
        {
            var sourceFile = _vfs.GetFile(path);
            if (sourceFile == null)
            {
                throw new FileNotFoundException($"Source file not found: {path}");
            }

            string sourceDir = Path.GetDirectoryName(path);
            string newPath = Path.Combine(sourceDir, newName);

            if (_vfs.GetFile(newPath) != null)
            {
                throw new IOException($"File already exists: {newPath}");
            }

            string content = ReadFile(path);
            WriteFile(newPath, content);
            DeleteFile(path);

            Console.WriteLine($"File renamed successfully to: {newPath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Rename failed: {ex.Message}");
            throw new FileSystemException($"Failed to rename file: {path} to {newName}", ex);
        }
    }

    public bool CopyFile(string sourcePath, string destinationPath)
    {
        EnsureNotSystemPath(sourcePath);
        EnsureNotSystemPath(destinationPath);

        try
        {
            string content = ReadFile(sourcePath);
            WriteFile(destinationPath, content);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to copy file: {ex.Message}");
            throw new FileSystemException($"Failed to copy file from {sourcePath} to {destinationPath}", ex);
        }
    }

    public bool MoveFile(string sourcePath, string destinationPath)
    {
        EnsureNotSystemPath(sourcePath);
        EnsureNotSystemPath(destinationPath);

        try
        {
            var sourceFile = _vfs.GetFile(sourcePath);
            if (sourceFile == null)
            {
                throw new FileNotFoundException($"Source file not found: {sourcePath}");
            }

            string destDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                VFSManager.CreateDirectory(destDir);
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

    // IDirectoryManager implementation
    public bool CreateDirectory(string path)
    {
        EnsureNotSystemPath(path);

        try
        {
            VFSManager.CreateDirectory(path);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            throw new FileSystemException($"Operation failed: {ex.Message}", ex);
        }
    }

    public bool DeleteDirectory(string path)
    {
        EnsureNotSystemPath(path);

        try
        {
            VFSManager.DeleteDirectory(path, true);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            throw new FileSystemException($"Operation failed: {ex.Message}", ex);
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
            Console.WriteLine($"[ERROR] {ex.Message}");
            throw new FileSystemException($"Operation failed: {ex.Message}", ex);
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
            Console.WriteLine($"[ERROR] {ex.Message}");
            throw new FileSystemException($"Operation failed: {ex.Message}", ex);
        }
    }

    public bool MoveDirectory(string sourcePath, string destinationPath)
    {
        EnsureNotSystemPath(sourcePath);
        EnsureNotSystemPath(destinationPath);

        try
        {
            if (!Directory.Exists(sourcePath))
            {
                throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");
            }

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));
            }

            foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Move(newPath, newPath.Replace(sourcePath, destinationPath));
            }

            Directory.Delete(sourcePath, true);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to move directory: {ex.Message}");
            throw new FileSystemException($"Failed to move directory from {sourcePath} to {destinationPath}", ex);
        }
    }

    public bool RenameDirectory(string path, string newName)
    {
        EnsureNotSystemPath(path);
        EnsureNotSystemPath(newName);

        try
        {
            string newPath = Path.Combine(Path.GetDirectoryName(path), newName);

            Directory.CreateDirectory(newPath);
            CopyDirectory(path, newPath);
            DeleteDirectory(path);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            throw new FileSystemException($"Operation failed: {ex.Message}", ex);
        }
    }

    public bool CopyDirectory(string sourcePath, string destinationPath)
    {
        EnsureNotSystemPath(sourcePath);
        EnsureNotSystemPath(destinationPath);

        try
        {
            if (!Directory.Exists(destinationPath))
            {
                VFSManager.CreateDirectory(destinationPath);
            }

            CopyDirectoryRecursive(sourcePath, destinationPath);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to copy directory: {ex.Message}");
            throw new FileSystemException($"Failed to copy directory from {sourcePath} to {destinationPath}", ex);
        }
    }

    private void CopyDirectoryRecursive(string sourcePath, string destinationPath)
    {
        var sourceDirectory = _vfs.GetDirectory(sourcePath);

        if (sourceDirectory != null)
        {
            var directoryEntries = _vfs.GetDirectoryListing(sourceDirectory);

            foreach (var entry in directoryEntries)
            {
                string sourceEntryPath = Path.Combine(sourcePath, entry.mName);
                string destinationEntryPath = Path.Combine(destinationPath, entry.mName);

                if (entry.mEntryType == DirectoryEntryTypeEnum.Directory)
                {
                    VFSManager.CreateDirectory(destinationEntryPath);

                    CopyDirectoryRecursive(sourceEntryPath, destinationEntryPath);
                }
                else if (entry.mEntryType == DirectoryEntryTypeEnum.File)
                {
                    CopyFile(sourceEntryPath, destinationEntryPath);
                }
            }
        }
        else
        {
            Console.WriteLine($"[ERROR] Source directory not found: {sourcePath}");
        }
    }

    // ICacheManager implementation
    public byte[] GetCachedFile(string path)
    {
        if (_fileCache.ContainsKey(path))
        {
            return _fileCache[path];
        }

        return null;
    }

    public void CacheFile(string path, byte[] content)
    {
        _fileCache[path] = content;
    }

    // IVFSManager implementation
    public long GetAvailableFreeSpace(string drive)
    {
        try
        {
            return _vfs.GetAvailableFreeSpace(drive);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            throw new FileSystemException($"Operation failed: {ex.Message}", ex);
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
            Console.WriteLine($"[ERROR] {ex.Message}");
            throw new FileSystemException($"Operation failed: {ex.Message}", ex);
        }
    }

    // Helper methods
    private bool IsSystemPath(string path)
    {
        return path.StartsWith(SystemDirectory, StringComparison.OrdinalIgnoreCase);
    }

    private void EnsureNotSystemPath(string path)
    {
        if (IsSystemPath(path))
        {
            throw new UnauthorizedAccessException("Operation not allowed in system directory.");
        }
    }
    
    // IDiskManager implementation
    public void FormatDrive(string name)
    {
        try
        {
            bool driveExists = false;
            foreach (var disk in _vfs.GetDisks())
            {
                int index = 0;
                foreach (var partition in disk.Partitions)
                {
                    if (partition.RootPath == name)
                    {
                        driveExists = true;
                        ConsoleManager.WriteLineColored($"Formatting partition {index} of disk {name}...",
                            ConsoleStyle.Colors.Warning);
                        disk.FormatPartition(0, "FAT32", true);
                        ConsoleManager.WriteLineColored("Format completed successfully.", ConsoleStyle.Colors.Success);
                        break;
                    }

                    index++;
                }
            }

            if (!driveExists)
            {
                ConsoleManager.WriteLineColored($"The drive \"{name}\" doesn't exist!", ConsoleStyle.Colors.Error);
            }
        }
        catch (Exception ex)
        {
            ConsoleManager.WriteLineColored($"Error formatting drive: {ex.Message}", ConsoleStyle.Colors.Error);
        }
    }

    public void CreatePartition(string diskLetter, int partitionSize)
    {
        try
        {
            foreach (var disk in _vfs.GetDisks())
            {
                foreach (var partition in disk.Partitions)
                {
                    if (partition.RootPath == diskLetter)
                    {
                        ConsoleManager.WriteLineColored(
                            $"Creating new partition on disk {diskLetter} with size {partitionSize}...",
                            ConsoleStyle.Colors.Primary);
                        disk.CreatePartition(partitionSize);
                        disk.FormatPartition(disk.Partitions.Count - 1, "FAT32", true);
                        disk.MountPartition(disk.Partitions.Count - 1);
                        ConsoleManager.WriteLineColored("Partition created successfully.", ConsoleStyle.Colors.Success);
                        return;
                    }
                }
            }

            ConsoleManager.WriteLineColored($"Disk {diskLetter} not found.", ConsoleStyle.Colors.Error);
        }
        catch (Exception ex)
        {
            ConsoleManager.WriteLineColored($"Error creating partition: {ex.Message}", ConsoleStyle.Colors.Error);
        }
    }

    public void ListPartitions(string driveLetter)
    {
        try
        {
            bool found = false;
            foreach (var disk in _vfs.GetDisks())
            {
                foreach (var partition in disk.Partitions)
                {
                    if (partition.RootPath == driveLetter)
                    {
                        found = true;
                        ConsoleManager.WriteLineColored("Partition Information:", ConsoleStyle.Colors.Primary);
                        ConsoleManager.WriteLineColored($"Root Path: {partition.RootPath}", ConsoleStyle.Colors.Accent);
                        ConsoleManager.WriteLineColored($"Has FileSystem: {partition.HasFileSystem}",
                            ConsoleStyle.Colors.Accent);
                        ConsoleManager.WriteLineColored($"Label: {partition.MountedFS.Label}",
                            ConsoleStyle.Colors.Accent);
                    }
                }
            }

            if (!found)
            {
                ConsoleManager.WriteLineColored($"No partitions found for drive {driveLetter}",
                    ConsoleStyle.Colors.Warning);
            }
        }
        catch (Exception ex)
        {
            ConsoleManager.WriteLineColored($"Error listing partitions: {ex.Message}", ConsoleStyle.Colors.Error);
        }
    }
}