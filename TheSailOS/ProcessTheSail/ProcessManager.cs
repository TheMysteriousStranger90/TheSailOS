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
        _scheduler = new Scheduler();
        _processTable = new Dictionary<int, Process>();
    }

    public SchedulerStatistics GetSchedulerStatistics()
    {
        return _scheduler.GetStatistics();
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

    public ProcessSnapshot GetProcessSnapshot(int pid)
    {
        return _processTable.TryGetValue(pid, out var process)
            ? process.CreateSnapshot()
            : null;
    }

    public void SetProcessPriority(int pid, int newPriority)
    {
        if (_processTable.TryGetValue(pid, out var process))
        {
            process.UpdatePriority(newPriority);
            _scheduler.RescheduleProcess(process);
        }
    }

    public List<ProcessSnapshot> GetAllProcessSnapshots()
    {
        return _processes.Select(p => p.CreateSnapshot()).ToList();
    }

    public ProcessStatistics GetProcessStatistics(int pid)
    {
        return _processTable.TryGetValue(pid, out var process)
            ? process.Statistics
            : null;
    }
}