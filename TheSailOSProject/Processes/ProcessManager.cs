using System.Collections.Generic;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Processes;

public static class ProcessManager
{
    private static List<Process> _processes;
    private static ulong _nextProcessId = 0;

    public static void Initialize()
    {
        ConsoleManager.WriteLineColored("[ProcessManager] Initialized", ConsoleStyle.Colors.Success);
        _processes = new List<Process>();
    }

    public static Process Register(Process process)
    {
        if (_processes.Contains(process))
        {
            ConsoleManager.WriteLineColored($"[ProcessManager] Process {process.Name} already registered.",
                ConsoleStyle.Colors.Warning);
            return process;
        }

        process.SetId(_nextProcessId++);
        _processes.Add(process);
        ConsoleManager.WriteLineColored($"[ProcessManager] Registered process: {process.Name} (ID: {process.Id})",
            ConsoleStyle.Colors.Primary);
        return process;
    }

    public static bool Unregister(Process process)
    {
        bool removed = _processes.Remove(process);
        if (removed)
        {
            ConsoleManager.WriteLineColored($"[ProcessManager] Unregistered process: {process.Name} (ID: {process.Id})",
                ConsoleStyle.Colors.Primary);
        }
        else
        {
            ConsoleManager.WriteLineColored($"[ProcessManager] Process {process.Name} not found.",
                ConsoleStyle.Colors.Warning);
        }

        return removed;
    }

    public static bool Start(Process process)
    {
        if (!_processes.Contains(process))
        {
            ConsoleManager.WriteLineColored($"[ProcessManager] Process {process.Name} not registered.",
                ConsoleStyle.Colors.Warning);
            return false;
        }

        if (process.IsRunning)
        {
            ConsoleManager.WriteLineColored($"[ProcessManager] Process {process.Name} already running.",
                ConsoleStyle.Colors.Warning);
            return true;
        }

        process.TryStart();
        return process.IsRunning;
    }

    public static bool Stop(Process process)
    {
        if (!_processes.Contains(process))
        {
            ConsoleManager.WriteLineColored($"[ProcessManager] Process {process.Name} not registered.",
                ConsoleStyle.Colors.Warning);
            return false;
        }

        if (!process.IsRunning)
        {
            ConsoleManager.WriteLineColored($"[ProcessManager] Process {process.Name} not running.",
                ConsoleStyle.Colors.Warning);
            return true;
        }

        process.TryStop();
        return !process.IsRunning;
    }

    public static void Update()
    {
        foreach (var process in _processes)
        {
            if (process.IsRunning)
            {
                process.TryRun();
            }
        }
    }

    public static Process GetProcessById(ulong id)
    {
        foreach (var process in _processes)
        {
            if (process.Id == id)
            {
                return process;
            }
        }

        return null;
    }

    public static Process GetProcessByName(string name)
    {
        foreach (var process in _processes)
        {
            if (process.Name == name)
            {
                return process;
            }
        }

        return null;
    }

    public static List<Process> GetProcesses()
    {
        return new List<Process>(_processes);
    }
}