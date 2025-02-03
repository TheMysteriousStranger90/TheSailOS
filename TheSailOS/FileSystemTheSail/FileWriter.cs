using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Cosmos.System.FileSystem.Listing;

namespace TheSailOS.FileSystemTheSail;

public class FileWriter
{
    private FileTheSail _fileTheSail;

    public FileWriter(FileTheSail fileTheSail)
    {
        _fileTheSail = fileTheSail;
    }

    public void WriteFile(string path, string content)
    {
        try
        {
            path = Path.Combine(Kernel.CurrentDirectory, path);

            // Check if file exists and get correct file type
            var file = _fileTheSail._vfs.GetFile(path);
            if (file != null)
            {
                Console.WriteLine($"File {path} already exists, updating content...");
                UpdateFileContent(file, content);
                return;
            }

            // Create new file
            Console.WriteLine($"Creating new file: {path}");
            file = _fileTheSail._vfs.CreateFile(path);
            
            Thread.Sleep(100); // Wait for filesystem

            if (file == null)
            {
                throw new IOException($"Failed to create file: {path}");
            }

            // Write initial content
            if (!string.IsNullOrEmpty(content))
            {
                UpdateFileContent(file, content);
            }

            Console.WriteLine($"File created successfully: {path}");
        }
        catch (Exception ex)
        {
            throw new IOException($"Error in WriteFile: {ex.Message}");
        }
    }

    private void UpdateFileContent(Cosmos.System.FileSystem.Listing.DirectoryEntry file, string content)
    {
        try
        {
            using (var stream = file.GetFileStream())
            {
                if (!stream.CanWrite)
                    throw new IOException("Cannot write to file stream");

                stream.Position = 0;
                stream.SetLength(0);

                if (!string.IsNullOrEmpty(content))
                {
                    var bytes = Encoding.ASCII.GetBytes(content);
                    stream.Write(bytes, 0, bytes.Length);
                }

                stream.Flush();
            }
        }
        catch (Exception ex)
        {
            throw new IOException($"Error updating file content: {ex.Message}");
        }
    }

    public void SaveFile(string path, byte[] content)
    {
        path = Path.Combine(Kernel.CurrentDirectory, path);
        var file = _fileTheSail._vfs.GetFile(path) ?? _fileTheSail._vfs.CreateFile(path);
        using (var stream = file.GetFileStream())
        {
            if (stream.CanWrite)
            {
                stream.Write(content, 0, content.Length);
            }
            else
            {
                Console.WriteLine($"Cannot write to file {path}");
            }
        }
    }

    public void DeleteFile(string path)
    {
        path = Path.Combine(Kernel.CurrentDirectory, path);
        var file = _fileTheSail._vfs.GetFile(path);
        if (file != null)
        {
            _fileTheSail._vfs.DeleteFile(file);
        }
        else
        {
            throw new FileNotFoundException($"File {path} not found");
        }
    }
    
    public List<string> GetFileListing(string path)
    {
        path = Path.Combine(Kernel.CurrentDirectory, path);
        var directory = _fileTheSail._vfs.GetDirectory(path);
        if (directory == null)
        {
            throw new DirectoryNotFoundException($"Directory {path} not found");
        }

        var listing = _fileTheSail._vfs.GetDirectoryListing(directory);

        return listing.Where(entry => entry.mEntryType == DirectoryEntryTypeEnum.File).Select(entry => entry.mName)
            .ToList();
    }
}