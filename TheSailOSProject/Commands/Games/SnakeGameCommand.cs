using System;
using TheSailOSProject.Games.Snake;

namespace TheSailOSProject.Commands.Games;

public class SnakeGameCommand : ICommand
{
    public void Execute(string[] args)
    {
        Console.Clear();
        Console.WriteLine("Starting Snake Game...");
        Console.WriteLine("Use arrow keys to move, ESC to exit");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);

        var game = new SnakeGame();
        game.Run();

        Console.Clear();
    }
}