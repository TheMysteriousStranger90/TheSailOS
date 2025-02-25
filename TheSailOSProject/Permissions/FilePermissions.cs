namespace TheSailOSProject.Permissions;

public class FilePermissions
{
    public string FilePath { get; set; }
    public string OwnerUsername { get; set; }
    public bool AllowRead { get; set; }
    public bool AllowWrite { get; set; }

    public FilePermissions(string filePath, string ownerUsername, bool allowRead = true, bool allowWrite = true)
    {
        FilePath = filePath;
        OwnerUsername = ownerUsername;
        AllowRead = allowRead;
        AllowWrite = allowWrite;
    }
}