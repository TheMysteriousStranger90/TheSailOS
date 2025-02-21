using System.Collections.Generic;

namespace TheSailOSProject.FileSystem;

public interface IFileManager
{
    string ReadFile(string path);
    bool CreateFileTheSail(string path);
    bool DeleteFileTheSail(string path);
    bool WriteFile(string path, string content);
    bool WriteFile(string path, List<string> content);
    bool RenameFile(string path, string newName);
    bool CopyFile(string sourcePath, string destinationPath);
    bool MoveFile(string sourcePath, string destinationPath);
}