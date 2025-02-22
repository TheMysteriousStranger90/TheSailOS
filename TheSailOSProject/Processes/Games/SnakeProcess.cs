using System;
using TheSailOSProject.Games.Snake;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Processes.Games;

public class SnakeProcess : Process
{
    private SnakeGame _game;
    private bool _exitGame = false;

    public SnakeProcess() : base("SnakeGame", ProcessType.Application)
    {
        _game = new SnakeGame();
    }

    public override void Start()
    {
        base.Start();
        Console.Clear();
        Console.WriteLine("Starting Snake Game...");
        Console.WriteLine("Use arrow keys to move, ESC to exit");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);
    }

    public override void Run()
    {
        _game.Run();
        Stop();
    }

    public override void Stop()
    {
        base.Stop();
        Console.Clear();
        ConsoleManager.WriteLineColored("[SnakeGame] Exited", ConsoleStyle.Colors.Primary);
    }
}