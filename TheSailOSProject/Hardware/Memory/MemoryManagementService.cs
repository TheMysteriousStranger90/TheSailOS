using System;
using TheSailOSProject.Processes;

namespace TheSailOSProject.Hardware.Memory;

public class MemoryManagementService : Process
{
    private const int CollectionInterval = 10000;
    private int _ticksSinceLastCollection;

    public MemoryManagementService() : base("MemoryManagementService", ProcessType.Service)
    {
        _ticksSinceLastCollection = 0;
    }

    public override void Run()
    {
        _ticksSinceLastCollection++;

        if (_ticksSinceLastCollection >= CollectionInterval)
        {
            CollectMemory();
            _ticksSinceLastCollection = 0;
        }
    }

    private void CollectMemory()
    {
        try
        {
            Cosmos.Core.Memory.Heap.Collect();

            MemoryManager.LogMemoryStatistics();

            Console.WriteLine("[MemoryManagementService] Garbage collection completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MemoryManagementService] Error during garbage collection: {ex.Message}");
        }
    }
}