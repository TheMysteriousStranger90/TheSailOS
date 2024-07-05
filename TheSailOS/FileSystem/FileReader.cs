using System;
using System.IO;
using System.Text;

namespace TheSailOS.FileSystem;

public class FileReader
{
    private FileTheSail _fileTheSail;

    public FileReader(FileTheSail fileTheSail)
    {
        _fileTheSail = fileTheSail;
    }

    public string ReadFile(string path)
    {
        path = Path.Combine(Kernel.CurrentDirectory, path);

        var file = _fileTheSail._vfs.GetFile(path);
        if (file == null)
        {
            throw new FileNotFoundException($"File {path} not found");
        }

        using (var stream = file.GetFileStream())
        {
            if (stream.CanRead)
            {
                byte[] textBytes = new byte[stream.Length];
                stream.Read(textBytes, 0, textBytes.Length);
                return Encoding.ASCII.GetString(textBytes);
            }
            else
            {
                throw new Exception($"Cannot read from file {path}");
            }
        }
    }

    public bool FileExists(string path)
    {
        path = Path.Combine(Kernel.CurrentDirectory, path);

        var file = _fileTheSail._vfs.GetFile(path);
        return file != null;
    }
}