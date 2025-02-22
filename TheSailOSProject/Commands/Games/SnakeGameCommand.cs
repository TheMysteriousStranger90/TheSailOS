using System.Threading;
using TheSailOSProject.Processes;
using TheSailOSProject.Processes.Games;


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