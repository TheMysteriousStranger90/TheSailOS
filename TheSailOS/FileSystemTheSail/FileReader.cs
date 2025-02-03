using System;
using System.IO;
using System.Text;

namespace TheSailOS.FileSystemTheSail;

public class FileReader
{
    private FileTheSail _fileTheSail;

    public FileReader(FileTheSail fileTheSail)
    {
        _fileTheSail = fileTheSail;
    }

    public string ReadFile(string path)
    {
        try
        {
            path = Path.Combine(Kernel.CurrentDirectory, path);
            var file = _fileTheSail._vfs.GetFile(path);
        
            if (file == null)
                throw new FileNotFoundException($"File {path} not found");

            using (var stream = file.GetFileStream())
            {
                if (!stream.CanRead)
                    throw new IOException($"Cannot read from file {path}");

                stream.Position = 0;
                byte[] textBytes = new byte[stream.Length];
                var bytesRead = stream.Read(textBytes, 0, textBytes.Length);
            
                if (bytesRead == 0)
                    return string.Empty;
                
                return Encoding.ASCII.GetString(textBytes, 0, bytesRead);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
            throw;
        }
    }
}