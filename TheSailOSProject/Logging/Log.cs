using System;
using System.Collections.Generic;
using System.IO;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Logging
{
    public class Log
    {
        private static readonly string LogFilePath = "0:\\System\\Logs\\system.log";
        private static readonly object LockObject = new object();
        private static bool _initialized = false;
        
        public static void Initialize()
        {
            if (_initialized)
                return;
                
            try
            {
                string logDir = Path.GetDirectoryName(LogFilePath);
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                
                if (!File.Exists(LogFilePath))
                {
                    using (File.Create(LogFilePath)) { }
                }
                
                WriteLog(LogPriority.Info, "System", "Logging system initialized");
                _initialized = true;
                
                ConsoleManager.WriteLineColored("[Logging] System initialized with RTC time source", ConsoleStyle.Colors.Success);
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"[ERROR] Failed to initialize logging system: {ex.Message}", 
                    ConsoleStyle.Colors.Error);
            }
        }
        
        public static void WriteLog(int priority, string source, string message, string username = "SYSTEM")
        {
            var logEvent = new LogEvent(priority, source, message, username);
            WriteLog(logEvent);
        }
        
        public static void WriteLog(LogEvent logEvent)
        {
            if (!_initialized)
            {
                Console.WriteLine("Warning: Logging system not initialized");
                return;
            }
            
            try
            {
                lock (LockObject)
                {
                    File.AppendAllText(LogFilePath, logEvent.ToString() + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log: {ex.Message}");
            }
        }
        
        public static List<LogEvent> ReadLog(int maxEntries = 100, int? minPriority = null)
        {
            var logEvents = new List<LogEvent>();
            
            if (!_initialized || !File.Exists(LogFilePath))
                return logEvents;
                
            try
            {
                var lines = File.ReadAllLines(LogFilePath);
                int startIndex = Math.Max(0, lines.Length - maxEntries);
                
                for (int i = startIndex; i < lines.Length; i++)
                {
                    if (LogEvent.TryParse(lines[i], out LogEvent logEvent))
                    {
                        if (minPriority == null || logEvent.Priority >= minPriority)
                        {
                            logEvents.Add(logEvent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read log: {ex.Message}");
            }
            
            return logEvents;
        }
        
        public static void Clear()
        {
            if (!_initialized)
                return;
                
            try
            {
                File.WriteAllText(LogFilePath, string.Empty);
                WriteLog(LogPriority.Info, "System", "Log file cleared");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to clear log: {ex.Message}");
            }
        }
    }
}