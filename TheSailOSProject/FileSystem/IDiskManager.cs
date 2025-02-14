namespace TheSailOSProject.FileSystem;

public interface IDiskManager
{
    void FormatDrive(string name);
    void CreatePartition(string diskLetter, int partitionSize);
    void ListPartitions(string driveLetter);
}