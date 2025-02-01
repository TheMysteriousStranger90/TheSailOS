namespace TheSailOS.MemoryTheSail;

public class MemorySegment
{
    public uint BaseAddress { get; set; }
    public uint Limit { get; set; }
    public MemoryPermissions Permissions { get; set; }
    public bool IsPresent { get; set; }
}