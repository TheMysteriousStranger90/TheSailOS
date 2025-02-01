namespace TheSailOS.MemoryTheSail;

public class PageStatistics
{
    public uint TotalPages { get; set; }
    public uint UsedPages { get; set; }
    public uint FreePages { get; set; }
    public uint SwappedPages { get; set; }
}