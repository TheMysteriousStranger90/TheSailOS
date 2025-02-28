using System;

namespace TheSailOSProject.Styles;

public static class ConsoleStyle
{
    public static class Colors
    {
        public static ConsoleColor Primary = ConsoleColor.DarkMagenta;
        public static ConsoleColor Secondary = ConsoleColor.DarkRed;
        public static ConsoleColor Accent = ConsoleColor.DarkGray;
        public static ConsoleColor Error = ConsoleColor.Red;
        public static ConsoleColor Warning = ConsoleColor.DarkYellow;
        public static ConsoleColor Success = ConsoleColor.DarkGreen;
        
        
        public static ConsoleColor Command = ConsoleColor.Magenta;
        public static ConsoleColor FilePath = ConsoleColor.Blue;
        public static ConsoleColor Flag = ConsoleColor.DarkCyan;
        public static ConsoleColor Number = ConsoleColor.Red;
        public static ConsoleColor String = ConsoleColor.White;
    }

    public static class Symbols
    {
        public const string Error = "✖";
        public const string Warning = "⚠";
        public const string Success = "✓";
        public const string Info = "ℹ";
        public const string Prompt = ">";
    }
}