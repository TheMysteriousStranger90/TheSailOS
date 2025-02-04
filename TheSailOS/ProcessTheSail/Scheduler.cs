using System.Collections.Generic;

namespace TheSailOS.ProcessTheSail;

public class Scheduler
{
    private readonly List<Queue<Process>> _priorityQueues;
    private readonly List<Process> _blockedProcesses;
    private Process _currentProcess;
    private readonly SchedulerStatistics _statistics;
    private const int MAX_PRIORITY_LEVELS = 10;

    public Scheduler()
    {
        _priorityQueues = new List<Queue<Process>>();
        for (int i = 0; i < MAX_PRIORITY_LEVELS; i++)
        {
            _priorityQueues.Add(new Queue<Process>());
        }

        _blockedProcesses = new List<Process>();
        _statistics = new SchedulerStatistics();
    }

    private void EnqueueProcess(Process process)
    {
        int priority = System.Math.Min(process.Priority, MAX_PRIORITY_LEVELS - 1);
        _priorityQueues[priority].Enqueue(process);
    }

    private void HandleTerminatedProcess()
    {
        if (_currentProcess.State == ProcessState.Terminated)
        {
            _statistics.TerminatedProcesses++;
            _currentProcess = null;
        }
    }

    public void Schedule()
    {
        if (_currentProcess != null)
        {
            if (_currentProcess.HasExceededTimeQuantum())
            {
                PreemptCurrentProcess();
            }
            else if (_currentProcess.State == ProcessState.Terminated)
            {
                HandleTerminatedProcess();
            }
        }

        if (_currentProcess == null)
        {
            SelectNextProcess();
        }

        if (_currentProcess != null)
        {
            _statistics.UpdateSchedulingStatistics(_currentProcess);
            _currentProcess.Execute();
        }
    }

    private void PreemptCurrentProcess()
    {
        _currentProcess.UpdateState(ProcessState.Ready);
        EnqueueProcess(_currentProcess);
        _statistics.Preemptions++;
        _currentProcess = null;
    }

    private void SelectNextProcess()
    {
        for (int i = MAX_PRIORITY_LEVELS - 1; i >= 0; i--)
        {
            if (_priorityQueues[i].Count > 0)
            {
                _currentProcess = _priorityQueues[i].Dequeue();
                _currentProcess.UpdateState(ProcessState.Running);
                _statistics.ContextSwitches++;
                break;
            }
        }
    }

    public void AddProcess(Process process)
    {
        process.UpdateState(ProcessState.Ready);
        EnqueueProcess(process);
    }

    public void RescheduleProcess(Process process)
    {
        if (process == _currentProcess)
        {
            PreemptCurrentProcess();
        }
        else if (_blockedProcesses.Contains(process))
        {
            UnblockProcess(process);
        }

        EnqueueProcess(process);
    }

    public void BlockProcess(Process process)
    {
        process.UpdateState(ProcessState.Blocked);
        _blockedProcesses.Add(process);
        _statistics.BlockedProcesses++;
    }

    public void UnblockProcess(Process process)
    {
        if (_blockedProcesses.Remove(process))
        {
            process.UpdateState(ProcessState.Ready);
            EnqueueProcess(process);
            _statistics.UnblockedProcesses++;
        }
    }

    public SchedulerStatistics GetStatistics() => _statistics;
}