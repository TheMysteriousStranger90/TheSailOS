using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CancellationTokenSource = System.Threading.CancellationTokenSource;

namespace TheSailOS.Tasks;

public class TaskManager
{
    private List<(System.Threading.Tasks.Task Task, int Priority, List<System.Threading.Tasks.Task> Dependencies, CancellationTokenSource Cts, IProgress<int> Progress, TimeSpan Timeout)> tasks = new List<(System.Threading.Tasks.Task, int, List<System.Threading.Tasks.Task>, CancellationTokenSource, IProgress<int>, TimeSpan)>();
    private object syncRoot = new object();

    public void AddTask(Func<CancellationToken, IProgress<int>, System.Threading.Tasks.Task> task, int priority, Action<int> reportProgress, TimeSpan timeout, List<System.Threading.Tasks.Task> dependencies = null)
    {
        var cts = new CancellationTokenSource();
        var progress = new Progress<int>(reportProgress);
        lock (syncRoot)
        {
            tasks.Add((task(cts.Token, progress), priority, dependencies, cts, progress, timeout));
        }
    }

    public async System.Threading.Tasks.Task Run()
    {
        while (true)
        {
            (System.Threading.Tasks.Task Task, int Priority, List<System.Threading.Tasks.Task> Dependencies, CancellationTokenSource Cts, IProgress<int> Progress, TimeSpan Timeout) taskToRun = default;

            lock (syncRoot)
            {
                if (!tasks.Any())
                {
                    break;
                }
                
                tasks.Sort((x, y) => y.Priority.CompareTo(x.Priority));
                
                for (int i = 0; i < tasks.Count; i++)
                {
                    var (task, _, dependencies, cts, _, _) = tasks[i];
                    if (dependencies == null || dependencies.All(d => d.IsCompleted))
                    {
                        taskToRun = tasks[i];
                        tasks.RemoveAt(i);
                        break;
                    }
                }
            }

            if (taskToRun.Task != null)
            {
                var delayTask = Task.Delay(taskToRun.Timeout);
                var completedTask = await Task.WhenAny(taskToRun.Task, delayTask);
                if (completedTask == delayTask)
                {
                    taskToRun.Cts.Cancel();
                }
            }
            else
            {
                await Task.Yield();
            }
        }
    }

    public void CancelTask(Task taskToCancel)
    {
        lock (syncRoot)
        {
            var taskEntry = tasks.FirstOrDefault(t => t.Task == taskToCancel);
            if (taskEntry != default)
            {
                taskEntry.Cts.Cancel();
            }
        }
    }
}