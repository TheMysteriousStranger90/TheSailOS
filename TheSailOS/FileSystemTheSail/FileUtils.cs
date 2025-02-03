using System.IO;

namespace TheSailOS.FileSystemTheSail;

public class FileUtils
{
    public static string GetParentPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar);
        }

        int lastSeparatorIndex = path.LastIndexOf(Path.DirectorySeparatorChar);
        if (lastSeparatorIndex <= 0)
        {
            return path + "\\";
        }

        if (lastSeparatorIndex == 2 && path[1] == ':')
        {
            return path.Substring(0, lastSeparatorIndex + 1);
        }

        return path.Substring(0, lastSeparatorIndex) + "\\";
    }

    public static string GetFreeSpace()
    {
        var availableSpace = Kernel.CurrentFileTheSail._vfs.GetAvailableFreeSpace(CurrentPathManager.CurrentDirectory);
        return ConvertSize(availableSpace);
    }

    public static string GetCapacity()
    {
        var totalSize = Kernel.CurrentFileTheSail._vfs.GetTotalSize(CurrentPathManager.CurrentDirectory);
        return ConvertSize(totalSize);
    }

    public static string ConvertSize(long bytes)
    {
        string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB" };
        double size = bytes;
        int i = 0;
        while (size >= 1024 && i < suffixes.Length - 1)
        {
            size /= 1024;
            i++;
        }
        return $"{size:0.##} {suffixes[i]}";
    }
}