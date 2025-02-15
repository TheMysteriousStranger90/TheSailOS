using System;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Processes;

public abstract class Process
{
    public ulong Id { get; private set; }
    public string Name { get; protected set; }
    public ProcessType Type { get; protected set; }
    public bool IsRunning { get; private set; }
    public System.DateTime Created { get; private set; }

    protected Process(string name, ProcessType type)
    {
        Name = name;
        Type = type;
        IsRunning = false;
        Created = System.DateTime.Now;
    }

    public virtual void Start()
    {
        IsRunning = true;
        ConsoleManager.WriteLineColored($"[Process] Started: {Name} (ID: {Id})", ConsoleStyle.Colors.Success);
    }

    public abstract void Run();

    public virtual void Stop()
    {
        IsRunning = false;
        ConsoleManager.WriteLineColored($"[Process] Stopped: {Name} (ID: {Id})", ConsoleStyle.Colors.Warning);
    }

    internal void SetId(ulong id)
    {
        Id = id;
    }

    internal void TryRun()
    {
        try
        {
            Run();
        }
        catch (Exception ex)
        {
            ConsoleManager.WriteLineColored($"[Process] {Name} (ID: {Id}) crashed: {ex.Message}",
                ConsoleStyle.Colors.Error);
            Stop();
        }
    }

    internal void TryStart()
    {
        try
        {
            Start();
        }
        catch (Exception ex)
        {
            ConsoleManager.WriteLineColored($"[Process] {Name} (ID: {Id}) failed to start: {ex.Message}",
                ConsoleStyle.Colors.Error);
            Stop();
        }
    }

    internal void TryStop()
    {
        try
        {
            Stop();
        }
        catch (Exception ex)
        {
            ConsoleManager.WriteLineColored($"[Process] {Name} (ID: {Id}) failed to stop: {ex.Message}",
                ConsoleStyle.Colors.Error);
        }
    }
}