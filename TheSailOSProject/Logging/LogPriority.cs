namespace TheSailOSProject.Logging
{
    public static class LogPriority
    {
        public const int Debug = 0;
        public const int Info = 1;
        public const int Warning = 2;
        public const int Error = 3;
        public const int Critical = 4;
        
        public static string ToString(int priority)
        {
            return priority switch
            {
                0 => "Debug",
                1 => "Info",
                2 => "Warning",
                3 => "Error",
                4 => "Critical",
                _ => "Unknown"
            };
        }
        
        public static int FromString(string priority)
        {
            return priority.ToLower() switch
            {
                "debug" => Debug,
                "info" => Info,
                "warning" => Warning,
                "error" => Error,
                "critical" => Critical,
                _ => Info
            };
        }
    }
}