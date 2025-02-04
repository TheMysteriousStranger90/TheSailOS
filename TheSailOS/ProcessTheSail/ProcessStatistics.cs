using System;

namespace TheSailOS.ProcessTheSail;

public class ProcessStatistics
{
    public int StateTransitions { get; set; }
    public int TimesScheduled { get; set; }
    public TimeSpan TotalWaitTime { get; set; }
    public TimeSpan TotalRunTime { get; set; }
    public int ContextSwitches { get; set; }
    public DateTime LastScheduled { get; set; }
    public float CpuUsagePercentage { get; set; }
}