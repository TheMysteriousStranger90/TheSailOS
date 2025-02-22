using System.Threading;
using TheSailOSProject.Processes;
using TheSailOSProject.Processes.Games;

namespace TheSailOSProject.Commands.Games;

public class TetrisGameCommand : ICommand
{
    public void Execute(string[] args)
    {
        var tetrisProcess = new TetrisProcess();
        ProcessManager.Register(tetrisProcess);
        ProcessManager.Start(tetrisProcess);

        while (tetrisProcess.IsRunning)
        {
            ProcessManager.Update();
            Thread.Sleep(100);
        }
    }
}