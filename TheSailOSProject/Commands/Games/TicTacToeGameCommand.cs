using System.Threading;
using TheSailOSProject.Games.TicTacToe;
using TheSailOSProject.Processes;
using TheSailOSProject.Processes.Games;

namespace TheSailOSProject.Commands.Games;

public class TicTacToeGameCommand : ICommand
{
    public void Execute(string[] args)
    {
        var ticTacToeProcess = new TicTacToeProcess();
        ProcessManager.Register(ticTacToeProcess);
        ProcessManager.Start(ticTacToeProcess);

        while (ticTacToeProcess.IsRunning)
        {
            ProcessManager.Update();
            Thread.Sleep(100);
        }
    }
}