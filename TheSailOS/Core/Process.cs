using System;
using System.Collections.Generic;
using TheSailOS.Core.Enums;

namespace TheSailOS.Core;

public abstract class Process
{
    public ulong Id { get; protected internal set; }
    public string Name { get; protected set; }
    public ProcessType Type { get; protected set; }
    public DateTime Created { get; protected set; } = DateTime.Now;
    public bool IsRunning { get; protected set; } = false;
    public bool Critical { get; protected set; } = false;
    public Process Parent { get; protected internal set; }
    public List<string> Args { get; protected set; } = new List<string>();

    protected Process(string name, ProcessType type, Process parent = null)
    {
        Name = name;
        Type = type;
        Parent = parent;
    }

    public virtual void Start()
    {
        if (IsRunning) return;

        IsRunning = true;
        OnStart();
    }

    protected virtual void OnStart()
    {
        // Custom start logic for derived classes
    }

    public abstract void Run();

    public virtual void Stop()
    {
        if (!IsRunning) return;

        IsRunning = false;
        OnStop();
    }

    protected virtual void OnStop()
    {
        // Custom stop logic for derived classes
    }

    public void TryRun()
    {
        try
        {
            Run();
        }
        catch (Exception e)
        {
            HandleException(e, "running");
        }
    }

    public void TryStart()
    {
        try
        {
            Start();
        }
        catch (Exception e)
        {
            HandleException(e, "starting");
        }
    }

    public void TryStop()
    {
        try
        {
            Stop();
        }
        catch (Exception e)
        {
            IsRunning = false;
            HandleException(e, "stopping");
        }
    }

    private void HandleException(Exception e, string action)
    {
        if (Critical)
        {
            Console.WriteLine($"Critical error while {action}: {e.Message}. System must shut down.");
        }
        else
        {
            Console.WriteLine($"Non-critical error occurred while {action}: {e.Message}. Continuing operation.");
        }
    }
}