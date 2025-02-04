using System;

namespace TheSailOS.ProcessTheSail;

public class SchedulerStatistics
{
    public int ContextSwitches { get; set; }
    public int Preemptions { get; set; }
    public int BlockedProcesses { get; set; }
    public int UnblockedProcesses { get; set; }
    public int TerminatedProcesses { get; set; }
    public TimeSpan TotalSchedulingTime { get; private set; }

    public void UpdateSchedulingStatistics(Process process)
    {
        TotalSchedulingTime += process.Context.ExecutionTime;
    }
}