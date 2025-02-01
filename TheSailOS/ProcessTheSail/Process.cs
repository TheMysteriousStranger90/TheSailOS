using System;
using System.Collections.Generic;

namespace TheSailOS.ProcessTheSail;

public class Process
{
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
    }

    public void Execute()
    {
        _startTime = DateTime.Now;
        State = ProcessState.Running;
        _processAction?.Invoke();
        Context.ExecutionTime += DateTime.Now - _startTime;
        _cpuBurst++;
    }

    public bool HasExceededTimeQuantum()
    {
        return (DateTime.Now - _startTime).TotalMilliseconds > Context.TimeQuantum;
    }
}