using System;

namespace TheSailOSProject.Hardware.CPU;

public static class CPUManager
{
    public static CPUStatistics GetCPUStatistics()
    {
        string vendorName = Cosmos.Core.CPU.GetCPUVendorName();
        string brandString = Cosmos.Core.CPU.GetCPUBrandString();
        uint cycleSpeedMHz = 0;
        try
        {
            cycleSpeedMHz = (uint)(Cosmos.Core.CPU.GetCPUCycleSpeed() / (1024 * 1024));
        }
        catch
        {
            try
            {
                cycleSpeedMHz = (uint)(Cosmos.Core.CPU.EstimateCPUSpeedFromName(brandString) / (1024 * 1024));
            }
            catch
            {
                cycleSpeedMHz = 0;
            }
        }
        ulong uptime = Cosmos.Core.CPU.GetCPUUptime();

        return new CPUStatistics(
            vendorName,
            brandString,
            cycleSpeedMHz,
            uptime
        );
    }

    public static void LogCPUStatistics()
    {
        CPUStatistics stats = GetCPUStatistics();
        Console.WriteLine("CPU Statistics:");
        Console.WriteLine($"  Vendor: {stats.VendorName}");
        Console.WriteLine($"  Brand: {stats.BrandString}");
        Console.WriteLine($"  Cycle Speed: {stats.CycleSpeedMHz} MHz");
        Console.WriteLine($"  Uptime: {stats.Uptime} ticks");
    }
}