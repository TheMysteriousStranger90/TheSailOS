namespace TheSailOSProject.Hardware.CPU;

public struct CPUStatistics
{
    public string VendorName { get; }
    public string BrandString { get; }
    public uint CycleSpeedMHz { get; }
    public ulong Uptime { get; }

    internal CPUStatistics(string vendorName, string brandString, uint cycleSpeedMHz, ulong uptime)
    {
        VendorName = vendorName;
        BrandString = brandString;
        CycleSpeedMHz = cycleSpeedMHz;
        Uptime = uptime;
    }
}