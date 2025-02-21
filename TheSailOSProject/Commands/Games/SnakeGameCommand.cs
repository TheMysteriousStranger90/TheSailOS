using System;
using System.Threading;
using TheSailOSProject.Games.Snake;
using TheSailOSProject.Processes;
using TheSailOSProject.Processes.Applications;

namespace TheSailOSProject.Commands.Games;

public class SnakeGameCommand : ICommand
{
    public void Execute(string[] args)
    {
        var snakeProcess = new SnakeProcess();
        ProcessManager.Register(snakeProcess);
        ProcessManager.Start(snakeProcess);

        while (snakeProcess.IsRunning)
        {
            ProcessManager.Update();
            Thread.Sleep(100);
        }
    }
}