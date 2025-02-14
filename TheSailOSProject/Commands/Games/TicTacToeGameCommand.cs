using TheSailOSProject.Games.TicTacToe;

namespace TheSailOSProject.Commands.Games;

public class TicTacToeGameCommand : ICommand
{
    public void Execute(string[] args)
    {
        var game = new TicTacToeGame();
        game.Run();
    }
}