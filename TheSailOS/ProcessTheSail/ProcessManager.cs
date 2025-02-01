using System;
using System.Collections.Generic;
using System.Linq;

namespace TheSailOS.ProcessTheSail;

public class ProcessManager
{
    private readonly List<Process> _processes;
    private readonly Scheduler _scheduler;
    private readonly Dictionary<int, Process> _processTable;

    public ProcessManager()
    {
        _processes = new List<Process>();
        _scheduler = new Scheduler(this);
        _processTable = new Dictionary<int, Process>();
    }

    public Process CreateProcess(string name, Action action, int priority = 1)
    {
        var process = new Process(name, action, priority);
        _processes.Add(process);
        _processTable[process.PID] = process;
        _scheduler.AddProcess(process);
        return process;
    }

    public void CreateChildProcess(Process parent, string name, Action action)
    {
        var child = CreateProcess(name, action);
        child.Parent = parent;
        parent.Children.Add(child);
    }

    public void UpdateProcessState()
    {
        _scheduler.Schedule();
    }

    public bool BlockProcess(int pid)
    {
        if (_processTable.TryGetValue(pid, out Process process))
        {
            _scheduler.BlockProcess(process);
            return true;
        }
        return false;
    }

    public bool UnblockProcess(int pid)
    {
        if (_processTable.TryGetValue(pid, out Process process))
        {
            _scheduler.UnblockProcess(process);
            return true;
        }
        return false;
    }
    
    public void ListProcesses()
    {
        Console.WriteLine("\nRunning Processes:");
        Console.WriteLine("PID\tName\t\tState\t\tPriority");
        Console.WriteLine("----------------------------------------");
        
        foreach (var process in _processes)
        {
            Console.WriteLine($"{process.PID}\t{process.Name,-15}\t{process.State,-12}\t{process.Priority}");
            
            if (process.Children.Count > 0)
            {
                foreach (var child in process.Children)
                {
                    Console.WriteLine($"  └─{child.PID}\t{child.Name,-15}\t{child.State,-12}\t{child.Priority}");
                }
            }
        }
        Console.WriteLine($"\nTotal Processes: {_processes.Count}");
    }

    public ProcessStatistics GetProcessStatistics()
    {
        return new ProcessStatistics
        {
            TotalProcesses = _processes.Count,
            RunningProcesses = _processes.Count(p => p.State == ProcessState.Running),
            BlockedProcesses = _processes.Count(p => p.State == ProcessState.Blocked),
            ReadyProcesses = _processes.Count(p => p.State == ProcessState.Ready)
        };
    }
}