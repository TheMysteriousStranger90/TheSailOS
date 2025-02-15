namespace TheSailOSProject.Hardware.Memory;

public struct MemoryStatistics
{
    public uint TotalMB { get; }
    public uint UnavailableMB { get; }
    public uint KernelMB { get; }
    public uint UsedMB { get; }
    public uint FreeMB { get; }
    public uint PercentUsed { get; }

    internal MemoryStatistics(uint totalMB, uint unavailableMB, uint kernelMB, uint usedMB, uint freeMB,
        uint percentUsed)
    {
        TotalMB = totalMB;
        UnavailableMB = unavailableMB;
        KernelMB = kernelMB;
        UsedMB = usedMB;
        FreeMB = freeMB;
        PercentUsed = percentUsed;
    }
}