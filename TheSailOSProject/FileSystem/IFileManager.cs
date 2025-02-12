namespace TheSailOSProject.FileSystem;

public interface IFileManager
{
    string ReadFile(string path);
    bool CreateFile(string path);
    bool DeleteFile(string path);
    bool WriteFile(string path, string content);
    bool RenameFile(string path, string newName);
    bool CopyFile(string sourcePath, string destinationPath);
    bool MoveFile(string sourcePath, string destinationPath);
}