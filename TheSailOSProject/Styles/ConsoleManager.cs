

using System;

namespace TheSailOSProject.Styles;

public static class ConsoleManager
{
    private const int DEFAULT_WIDTH = 800;
    private const int DEFAULT_HEIGHT = 600;
    private static ConsoleColor _defaultForeground = ConsoleColor.White;
    private static ConsoleColor _defaultBackground = ConsoleColor.Black;

    public static void Initialize()
    {
        try
        {
            Console.Clear();
            SetDefaultColors();
            DrawLogo();
            
            try
            {
                Console.SetWindowSize(DEFAULT_WIDTH, DEFAULT_HEIGHT);
                Console.SetBufferSize(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            }
            catch
            {
                Console.WriteLine("[WARNING] Could not set console size");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to initialize console: {ex.Message}");
        }
    }

    private static void DrawLogo()
    {
        Console.Clear();
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkMagenta;

        string[] logo =
        {
            @" _______ _           ____        _ _ ___  ____ ",
            @"|__   __| |         / ___|  __ _(_) / _ \/ ___|",
            @"   | |  | |__   ___\___ \ / _` | | | | | \___ \",
            @"   | |  | '_ \ / _ \___) | (_| | | | |_| |___) |",
            @"   | |  | | | |  __/____/ \__,_|_|_|\___/|____/",
            @"   |_|  |_| |_|\___|"
        };
        
        int startY = 2;
        int startX = (Console.WindowWidth - logo[0].Length) / 2;
        
        for (int i = 0; i < logo.Length; i++)
        {
            Console.SetCursorPosition(Math.Max(0, startX), startY + i);
            Console.WriteLine(logo[i]);
        }
        
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        int lineY = startY + logo.Length + 1;
        DrawHorizontalLine(lineY);
        
        Console.ForegroundColor = ConsoleColor.White;
        Console.SetCursorPosition((Console.WindowWidth - 25) / 2, lineY + 1);
        Console.WriteLine($"TheSailOS v0.0.1 - {System.DateTime.Now.Year}");

        DrawHorizontalLine(lineY + 2);
        Console.WriteLine();

        Console.ForegroundColor = originalColor;
    }

    private static void DrawHorizontalLine(int y)
    {
        string line = new string('=', Console.WindowWidth);
        Console.SetCursorPosition(0, y);
        Console.WriteLine(line);
    }

    public static void SetDefaultColors()
    {
        Console.ForegroundColor = _defaultForeground;
        Console.BackgroundColor = _defaultBackground;
        Console.Clear();
    }
    
    public static void WriteColored(string text, ConsoleColor foreground, ConsoleColor? background = null)
    {
        var originalFore = Console.ForegroundColor;
        var originalBack = Console.BackgroundColor;

        Console.ForegroundColor = foreground;
        if (background.HasValue)
        {
            Console.BackgroundColor = background.Value;
        }

        Console.Write(text);

        Console.ForegroundColor = originalFore;
        Console.BackgroundColor = originalBack;
    }

    public static void WriteLineColored(string text, ConsoleColor foreground, ConsoleColor? background = null)
    {
        WriteColored(text + Environment.NewLine, foreground, background);
    }
}