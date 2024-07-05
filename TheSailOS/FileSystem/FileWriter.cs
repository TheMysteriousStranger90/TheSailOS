using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;

namespace TheSailOS.FileSystem;

public class FileWriter
{
    private FileTheSail _fileTheSail;

    public FileWriter(FileTheSail fileTheSail)
    {
        _fileTheSail = fileTheSail;
    }

    public void WriteFile(string path, string content)
    {
        path = Path.Combine(Kernel.CurrentDirectory, path);
        var file = _fileTheSail._vfs.GetFile(path);
        if (file == null)
        {
            file = _fileTheSail._vfs.CreateFile(path);
        }

        using (var stream = file.GetFileStream())
        {
            if (stream.CanWrite)
            {
                byte[] textBytes = Encoding.ASCII.GetBytes(content);
                stream.Write(textBytes, 0, textBytes.Length);
            }
            else
            {
                throw new Exception($"Cannot write to file {path}");
            }
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

    public bool FileExists(string path)
    {
        path = Path.Combine(Kernel.CurrentDirectory, path);
        var file = _fileTheSail._vfs.GetFile(path);
        return file != null;
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