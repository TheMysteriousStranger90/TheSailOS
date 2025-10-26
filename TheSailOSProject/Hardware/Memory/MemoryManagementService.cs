using System;
using TheSailOSProject.Processes;

namespace TheSailOSProject.Hardware.Memory;

public class MemoryManagementService : Process
{
    private const string TimerName = "memory_gc_timer";
    private const ulong CollectionIntervalNs = 60000000000;
    private bool _isTimerRegistered = false;

    public MemoryManagementService() : base("MemoryManagementService", ProcessType.Service)
    {
    }

    public override void Start()
    {
        base.Start();
        
        if (!_isTimerRegistered)
        {
            bool created = TheSailOSProject.Hardware.Timer.TimerManager.CreateTimer(
                TimerName,
                CollectionIntervalNs,
                OnTimerTick,
                true,
                "Periodic garbage collection"
            );

            if (created)
            {
                _isTimerRegistered = true;
                Console.WriteLine("[MemoryManagementService] Timer registered successfully");
            }
            else
            {
                Console.WriteLine("[MemoryManagementService] Failed to register timer");
            }
        }
    }

    public override void Run()
    {
        if (_isTimerRegistered && 
            TheSailOSProject.Hardware.Timer.TimerManager.HasTimerTriggered(TimerName))
        {
            CollectMemory();
        }
    }

    public override void Stop()
    {
        if (_isTimerRegistered)
        {
            TheSailOSProject.Hardware.Timer.TimerManager.DestroyTimer(TimerName);
            _isTimerRegistered = false;
            Console.WriteLine("[MemoryManagementService] Timer unregistered");
        }
        
        base.Stop();
    }

    private void OnTimerTick()
    {
    }

    private void CollectMemory()
    {
        try
        {
            Cosmos.Core.Memory.Heap.Collect();
            
            var stats = MemoryManager.GetMemoryStatistics();
            Console.WriteLine($"[MemoryManagementService] GC completed. Free: {stats.FreeMB}MB, Used: {stats.UsedMB}MB ({stats.PercentUsed}%)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MemoryManagementService] GC error: {ex.Message}");
        }
    }
}