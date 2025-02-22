using System;
using TheSailOSProject.Games.TicTacToe;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Processes.Games;

public class TicTacToeProcess : Process
{
    private TicTacToeGame _game;

    public TicTacToeProcess() : base("TicTacToe", ProcessType.Application)
    {
        _game = new TicTacToeGame();
    }

    public override void Start()
    {
        base.Start();
        Console.Clear();
        Console.WriteLine("Starting Tic Tac Toe...");
        ConsoleManager.WriteLineColored("Controls:", ConsoleStyle.Colors.Success);
        ConsoleManager.WriteLineColored("Use numpad (1-9) to place X", ConsoleStyle.Colors.Accent);
        ConsoleManager.WriteLineColored("ESC to exit game", ConsoleStyle.Colors.Accent);
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
        ConsoleManager.WriteLineColored("[TicTacToe] Exited", ConsoleStyle.Colors.Primary);
    }
}