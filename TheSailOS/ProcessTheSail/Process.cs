using System;
using System.Collections.Generic;

namespace TheSailOS.ProcessTheSail;

public class Process
{
    private long _estimatedMemoryUsage;
    private const int BASE_MEMORY_PER_PROCESS = 1024;
    private static int _nextPid = 0;
    public int PID { get; private set; }
    public string Name { get; set; }
    public ProcessState State { get; set; }
    public int Priority { get; set; }
    public ProcessContext Context { get; private set; }
    public List<Process> Children { get; private set; }
    public Process Parent { get; set; }
    private Action _processAction;
    private DateTime _startTime;
    private int _cpuBurst;
    public DateTime CreationTime { get; private set; }
    public TimeSpan TotalCpuTime { get; private set; }
    public long MemoryUsage { get; private set; }
    public ProcessStatistics Statistics { get; private set; }
    public event Action<Process> OnStateChanged;

    public Process(string name, Action action, int priority = 1)
    {
        PID = _nextPid++;
        Name = name;
        State = ProcessState.New;
        Priority = priority;
        _processAction = action;
        Context = new ProcessContext();
        Children = new List<Process>();
        _cpuBurst = 0;
        CreationTime = DateTime.Now;
        TotalCpuTime = TimeSpan.Zero;
        Statistics = new ProcessStatistics();
        MemoryUsage = 0;
        _estimatedMemoryUsage = BASE_MEMORY_PER_PROCESS;
        MemoryUsage = _estimatedMemoryUsage;
    }

    public void Execute()
    {
        try
        {
            _startTime = DateTime.Now;
            State = ProcessState.Running;

            var startCpuTime = DateTime.Now;
            _processAction?.Invoke();

            var executionTime = DateTime.Now - startCpuTime;
            TotalCpuTime += executionTime;
            Context.ExecutionTime += executionTime;
            _cpuBurst++;

            UpdateMemoryUsage();

            UpdateStatistics(State, ProcessState.Ready);
        }
        catch (Exception ex)
        {
            State = ProcessState.Terminated;
        }
    }

    public bool HasExceededTimeQuantum()
    {
        if (_startTime == default(DateTime)) return false;
        var runningTime = DateTime.Now - _startTime;
        return runningTime.TotalMilliseconds > Context.TimeQuantum;
    }

    private void UpdateStatistics(ProcessState oldState, ProcessState newState)
    {
        Statistics.StateTransitions++;
        if (newState == ProcessState.Running)
        {
            Statistics.TimesScheduled++;
            Statistics.LastScheduled = DateTime.Now;
        }

        if (oldState == ProcessState.Running)
        {
            var runTime = DateTime.Now - Statistics.LastScheduled;
            Statistics.TotalRunTime += runTime;
            Statistics.CpuUsagePercentage = (float)(Statistics.TotalRunTime.TotalMilliseconds /
                (DateTime.Now - CreationTime).TotalMilliseconds * 100);
        }
    }

    public void UpdateState(ProcessState newState)
    {
        if (State != newState)
        {
            var oldState = State;
            State = newState;
            OnStateChanged?.Invoke(this);
            UpdateStatistics(oldState, newState);
        }
    }

    public void UpdatePriority(int newPriority)
    {
        if (newPriority >= 0)
        {
            Priority = newPriority;
            Context.TimeQuantum = CalculateTimeQuantum(newPriority);
        }
    }

    private int CalculateTimeQuantum(int priority)
    {
        return 100 - (priority * 10);
    }

    public ProcessSnapshot CreateSnapshot()
    {
        return new ProcessSnapshot
        {
            PID = PID,
            Name = Name,
            State = State,
            Priority = Priority,
            CpuTime = TotalCpuTime,
            MemoryUsage = MemoryUsage,
            CreationTime = CreationTime,
            ChildCount = Children.Count
        };
    }

    private void UpdateMemoryUsage()
    {
        _estimatedMemoryUsage = BASE_MEMORY_PER_PROCESS +
                                (_cpuBurst * 128) +
                                (Children.Count * 512);
        MemoryUsage = _estimatedMemoryUsage;
    }
}