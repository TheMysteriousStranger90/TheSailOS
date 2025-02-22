using System;
using TheSailOSProject.Games.Tetris;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Processes.Games;

public class TetrisProcess : Process
{
    private TetrisGame _game;

    public TetrisProcess() : base("Tetris", ProcessType.Application)
    {
        _game = new TetrisGame();
    }

    public override void Start()
    {
        base.Start();
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
        ConsoleManager.WriteLineColored("[Tetris] Exited", ConsoleStyle.Colors.Primary);
    }
}