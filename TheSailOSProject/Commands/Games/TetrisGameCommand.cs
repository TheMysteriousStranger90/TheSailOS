using System;
using TheSailOSProject.Games.Tetris;

namespace TheSailOSProject.Commands.Games;

public class TetrisGameCommand : ICommand
{
    public void Execute(string[] args)
    {
        Console.Clear();
        Console.WriteLine("Starting Tetris...");
        Console.WriteLine("Controls:");
        Console.WriteLine("← → : Move left/right");
        Console.WriteLine("↑ : Rotate");
        Console.WriteLine("↓ : Soft drop");
        Console.WriteLine("Space : Hard drop");
        Console.WriteLine("ESC : Exit");
        Console.WriteLine("\nPress any key to start...");
        Console.ReadKey(true);

        var game = new TetrisGame();
        game.Run();

        Console.Clear();
    }
}