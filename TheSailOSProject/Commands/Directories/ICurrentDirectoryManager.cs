namespace TheSailOSProject.Commands.Directories;

public interface ICurrentDirectoryManager
{
    string GetCurrentDirectory();
    void SetCurrentDirectory(string path);
    string CombinePath(string basePath, string relativePath);
    void ValidatePath(string path);
}