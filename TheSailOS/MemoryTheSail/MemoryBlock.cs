namespace TheSailOS.MemoryTheSail;

public class MemoryBlock
{
    public uint Address { get; set; }
    public uint Size { get; set; }
    public int PageCount { get; set; }
    public MemoryPermissions Permissions { get; set; }
    public bool IsGCManaged { get; set; }
    public int ReferenceCount { get; set; }
}