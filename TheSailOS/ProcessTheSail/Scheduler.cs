using System.Collections.Generic;

namespace TheSailOS.ProcessTheSail;

public class Scheduler
{
    private readonly Queue<Process> _readyQueue;
    private readonly List<Process> _blockedProcesses;
    private Process _currentProcess;
    private readonly ProcessManager _processManager;

    public Scheduler(ProcessManager processManager)
    {
        _readyQueue = new Queue<Process>();
        _blockedProcesses = new List<Process>();
        _processManager = processManager;
    }

    public void Schedule()
    {
        if (_currentProcess != null && _currentProcess.HasExceededTimeQuantum())
        {
            _currentProcess.State = ProcessState.Ready;
            _readyQueue.Enqueue(_currentProcess);
            _currentProcess = null;
        }

        if (_currentProcess == null && _readyQueue.Count > 0)
        {
            _currentProcess = _readyQueue.Dequeue();
            _currentProcess.Execute();
        }
    }

    public void AddProcess(Process process)
    {
        process.State = ProcessState.Ready;
        _readyQueue.Enqueue(process);
    }

    public void BlockProcess(Process process)
    {
        process.State = ProcessState.Blocked;
        _blockedProcesses.Add(process);
    }

    public void UnblockProcess(Process process)
    {
        if (_blockedProcesses.Remove(process))
        {
            process.State = ProcessState.Ready;
            _readyQueue.Enqueue(process);
        }
    }
}