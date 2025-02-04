using System;

namespace TheSailOS.ProcessTheSail;

public class ProcessSnapshot
{
    public int PID { get; set; }
    public string Name { get; set; }
    public ProcessState State { get; set; }
    public int Priority { get; set; }
    public TimeSpan CpuTime { get; set; }
    public long MemoryUsage { get; set; }
    public DateTime CreationTime { get; set; }
    public int ChildCount { get; set; }

    public override string ToString()
    {
        return $"Process {PID} ({Name}): " +
               $"State={State}, " +
               $"Priority={Priority}, " +
               $"CPU={CpuTime.TotalMilliseconds}ms, " +
               $"Memory={MemoryUsage}bytes, " +
               $"Children={ChildCount}";
    }
}