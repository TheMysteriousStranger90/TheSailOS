namespace TheSailOSProject.FileSystem;

public interface IDirectoryManager
{
    bool CreateDirectoryTheSail(string path);
    bool DeleteDirectoryTheSail(string path);
    string[] ListFiles(string path);
    string[] ListDirectories(string path);
    bool MoveDirectory(string sourcePath, string destinationPath);
    bool RenameDirectory(string path, string newName);
    bool CopyDirectory(string sourcePath, string destinationPath);
}