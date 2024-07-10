using System.Collections.Generic;
using System.Linq;

namespace TheSailOS.Core;

public class ProcessManager
{
    private List<Process> Processes { get; set; }
    private ulong _nextProcessId = 0;

    public void Initialize()
    {
        Processes = new List<Process>();
        _nextProcessId = 0;
    }

    public bool Register(Process process, Process parent = null)
    {
        if (Processes.Contains(process)) return false;

        process.Id = _nextProcessId++;
        process.Parent = parent;
        Processes.Add(process);
        return true;
    }

    public bool Unregister(Process process)
    {
        return Processes.Remove(process);
    }

    public bool Start(Process process)
    {
        if (Processes.Contains(process))
        {
            process.TryStart();
            return true;
        }

        return false;
    }

    public bool Stop(Process process)
    {
        if (Processes.Contains(process))
        {
            process.TryStop();
            return true;
        }

        return false;
    }

    public void Yield()
    {
        foreach (var process in Processes)
        {
            if (process.IsRunning)
            {
                process.TryRun();
            }
        }
    }

    public void Sweep()
    {
        Processes.RemoveAll(p => !p.IsRunning);
    }

    public Process GetProcessById(ulong id)
    {
        return Processes.FirstOrDefault(p => p.Id == id);
    }

    public Process GetProcessByName(string name)
    {
        return Processes.FirstOrDefault(p => p.Name == name);
    }

    public T GetProcess<T>() where T : Process
    {
        return Processes.OfType<T>().FirstOrDefault();
    }

    public void StopAll()
    {
        foreach (var process in Processes)
        {
            process.TryStop();
        }
    }
}