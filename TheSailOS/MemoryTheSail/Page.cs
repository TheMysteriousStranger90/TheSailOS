using System;

namespace TheSailOS.MemoryTheSail;

public class Page
{
    public const int PAGE_SIZE = 4096;
    public uint PageNumber { get; set; }
    public uint PhysicalAddress { get; set; }
    public bool IsPresent { get; set; }
    public bool IsDirty { get; set; }
    public DateTime LastAccessed { get; set; }
    public MemoryPermissions Permissions { get; set; }
}