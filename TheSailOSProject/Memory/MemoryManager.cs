using System;
using Cosmos.Core;

namespace TheSailOSProject.Memory;

public static class MemoryManager
{
    private static uint _totalMemoryMB;

    public static void Initialize()
    {
        _totalMemoryMB = CPU.GetAmountOfRAM();

        if (_totalMemoryMB < 64)
        {
            Console.WriteLine(
                "[ERROR] OUT OF MEMORY!\nYour system does not have enough RAM to run TheSailOSProject!\nAt least 64 Mb of RAM is required, but you only have " +
                _totalMemoryMB + " Mb.");
            Console.ReadKey(true);
            Cosmos.System.Power.Shutdown();
        }
    }

    public static MemoryStatistics GetMemoryStatistics()
    {
        uint memTotal = _totalMemoryMB;
        uint memUnavail = memTotal - (uint)GCImplementation.GetAvailableRAM();
        uint kernelMB = (CPU.GetEndOfKernel() / 1024 / 1024) + 1;
        uint memUsed = ((uint)GCImplementation.GetUsedRAM() / 1024 / 1024) + memUnavail + kernelMB;
        uint memFree = memTotal - memUsed;
        uint memPercentUsed = (uint)(((float)memUsed / (float)memTotal) * 100f);

        return new MemoryStatistics(
            memTotal,
            memUnavail,
            kernelMB,
            memUsed,
            memFree,
            memPercentUsed
        );
    }

    public static void LogMemoryStatistics()
    {
        MemoryStatistics stats = GetMemoryStatistics();
        Console.WriteLine("Memory Statistics:");
        Console.WriteLine($"  Total Memory: {stats.TotalMB} MB");
        Console.WriteLine($"  Unavailable Memory: {stats.UnavailableMB} MB");
        Console.WriteLine($"  Kernel Memory: {stats.KernelMB} MB");
        Console.WriteLine($"  Used Memory: {stats.UsedMB} MB");
        Console.WriteLine($"  Free Memory: {stats.FreeMB} MB");
        Console.WriteLine($"  Percentage Used: {stats.PercentUsed}%");
    }
}