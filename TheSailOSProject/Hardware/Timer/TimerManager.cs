using System;
using System.Collections.Generic;
using Cosmos.HAL;
using TheSailOSProject.Logging;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Hardware.Timer;

public static class TimerManager
{
    private static bool _isInitialized = false;
    private static readonly object _lockObject = new object();
    private static Dictionary<string, TimerInfo> _timers = new Dictionary<string, TimerInfo>();
    private static Dictionary<string, bool> _timerFlags = new Dictionary<string, bool>();
    private static Dictionary<string, int> _timerExecutionCount = new Dictionary<string, int>();

    private class TimerInfo
    {
        public PIT.PITTimer Timer { get; set; }
        public ulong Frequency { get; set; }
        public bool IsRecurring { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public System.DateTime LastTriggered { get; set; }
        public Action Callback { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
    }

    public static void Initialize()
    {
        lock (_lockObject)
        {
            if (_isInitialized)
            {
                Console.WriteLine("[TimerManager] Already initialized");
                return;
            }

            try
            {
                ConsoleManager.WriteLineColored("[TimerManager] Initializing timer system...",
                    ConsoleStyle.Colors.Primary);

                _isInitialized = true;

                Log.WriteLog(LogPriority.Info, "TimerManager",
                    "Timer system initialized successfully", "SYSTEM");
                ConsoleManager.WriteLineColored("[TimerManager] Timer system ready",
                    ConsoleStyle.Colors.Success);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogPriority.Error, "TimerManager",
                    $"Timer initialization failed: {ex.Message}", "SYSTEM");
                ConsoleManager.WriteLineColored(
                    $"[TimerManager] Initialization failed: {ex.Message}",
                    ConsoleStyle.Colors.Error);
                _isInitialized = false;
            }
        }
    }

    public static bool CreateTimer(string name, ulong frequency, Action callback,
        bool recurring, string description = "")
    {
        lock (_lockObject)
        {
            if (!_isInitialized)
            {
                ConsoleManager.WriteLineColored("[TimerManager] System not initialized",
                    ConsoleStyle.Colors.Error);
                return false;
            }

            try
            {
                if (_timers.ContainsKey(name))
                {
                    ConsoleManager.WriteLineColored(
                        $"[TimerManager] Timer '{name}' already exists",
                        ConsoleStyle.Colors.Warning);
                    return false;
                }

                if (string.IsNullOrEmpty(name))
                {
                    ConsoleManager.WriteLineColored("[TimerManager] Timer name cannot be empty",
                        ConsoleStyle.Colors.Error);
                    return false;
                }

                if (callback == null)
                {
                    ConsoleManager.WriteLineColored("[TimerManager] Callback cannot be null",
                        ConsoleStyle.Colors.Error);
                    return false;
                }

                Console.WriteLine(
                    $"[TimerManager] Creating timer '{name}' ({frequency}ns, recurring: {recurring})");

                Action wrappedCallback = () =>
                {
                    try
                    {
                        lock (_lockObject)
                        {
                            if (_timers.ContainsKey(name) && _timers[name].IsEnabled)
                            {
                                _timerFlags[name] = true;
                                _timers[name].LastTriggered = System.DateTime.Now;
                                _timerExecutionCount[name]++;
                            }
                        }

                        callback?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog(LogPriority.Error, "TimerManager",
                            $"Timer '{name}' callback error: {ex.Message}", "SYSTEM");
                    }
                };

                PIT.PITTimer timer = new PIT.PITTimer(wrappedCallback, frequency, recurring);
                Cosmos.HAL.Global.PIT.RegisterTimer(timer);

                var timerInfo = new TimerInfo
                {
                    Timer = timer,
                    Frequency = frequency,
                    IsRecurring = recurring,
                    CreatedAt = System.DateTime.Now,
                    LastTriggered = System.DateTime.MinValue,
                    Callback = callback,
                    Description = description,
                    IsEnabled = true
                };

                _timers.Add(name, timerInfo);
                _timerFlags.Add(name, false);
                _timerExecutionCount.Add(name, 0);

                Log.WriteLog(LogPriority.Info, "TimerManager",
                    $"Timer '{name}' created successfully", "SYSTEM");

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogPriority.Error, "TimerManager",
                    $"Failed to create timer '{name}': {ex.Message}", "SYSTEM");
                ConsoleManager.WriteLineColored(
                    $"[TimerManager] Failed to create timer '{name}': {ex.Message}",
                    ConsoleStyle.Colors.Error);
                return false;
            }
        }
    }

    public static bool DestroyTimer(string name)
    {
        lock (_lockObject)
        {
            try
            {
                if (!_timers.ContainsKey(name))
                {
                    ConsoleManager.WriteLineColored($"[TimerManager] Timer '{name}' not found",
                        ConsoleStyle.Colors.Warning);
                    return false;
                }

                var timerInfo = _timers[name];
                Cosmos.HAL.Global.PIT.UnregisterTimer(timerInfo.Timer.TimerID);

                _timers.Remove(name);
                _timerFlags.Remove(name);
                _timerExecutionCount.Remove(name);

                Console.WriteLine($"[TimerManager] Timer '{name}' destroyed");
                Log.WriteLog(LogPriority.Info, "TimerManager",
                    $"Timer '{name}' destroyed", "SYSTEM");

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogPriority.Error, "TimerManager",
                    $"Failed to destroy timer '{name}': {ex.Message}", "SYSTEM");
                ConsoleManager.WriteLineColored(
                    $"[TimerManager] Failed to destroy timer '{name}': {ex.Message}",
                    ConsoleStyle.Colors.Error);
                return false;
            }
        }
    }

    public static bool EnableTimer(string name)
    {
        lock (_lockObject)
        {
            if (!_timers.ContainsKey(name))
                return false;

            _timers[name].IsEnabled = true;
            Console.WriteLine($"[TimerManager] Timer '{name}' enabled");
            return true;
        }
    }

    public static bool DisableTimer(string name)
    {
        lock (_lockObject)
        {
            if (!_timers.ContainsKey(name))
                return false;

            _timers[name].IsEnabled = false;
            Console.WriteLine($"[TimerManager] Timer '{name}' disabled");
            return true;
        }
    }

    public static bool HasTimerTriggered(string name)
    {
        lock (_lockObject)
        {
            if (!_timerFlags.ContainsKey(name))
                return false;

            bool triggered = _timerFlags[name];
            _timerFlags[name] = false;
            return triggered;
        }
    }

    public static int GetExecutionCount(string name)
    {
        lock (_lockObject)
        {
            if (!_timerExecutionCount.ContainsKey(name))
                return 0;

            return _timerExecutionCount[name];
        }
    }

    public static void ListTimers()
    {
        lock (_lockObject)
        {
            if (_timers.Count == 0)
            {
                ConsoleManager.WriteLineColored("No active timers", ConsoleStyle.Colors.Warning);
                return;
            }

            ConsoleManager.WriteLineColored($"\nActive Timers ({_timers.Count}):",
                ConsoleStyle.Colors.Primary);
            Console.WriteLine(new string('=', 90));

            Console.WriteLine(string.Format("  {0,-20} {1,-12} {2,-10} {3,-15} {4}",
                "Name", "Status", "Executions", "Frequency", "Description"));
            Console.WriteLine(new string('-', 90));

            foreach (var kvp in _timers)
            {
                var name = kvp.Key;
                var info = kvp.Value;

                string status = info.IsEnabled ? "Enabled" : "Disabled";
                ConsoleColor statusColor = info.IsEnabled ? ConsoleColor.Green : ConsoleColor.Yellow;

                string freqStr = FormatFrequency(info.Frequency);
                int execCount = _timerExecutionCount[name];
                string desc = string.IsNullOrEmpty(info.Description)
                    ? "-"
                    : (info.Description.Length > 25
                        ? info.Description.Substring(0, 22) + "..."
                        : info.Description);

                Console.Write("  ");
                Console.Write(string.Format("{0,-20} ", name));

                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = statusColor;
                Console.Write(string.Format("{0,-12} ", status));
                Console.ForegroundColor = oldColor;

                Console.Write(string.Format("{0,-10} ", execCount));
                Console.Write(string.Format("{0,-15} ", freqStr));
                Console.WriteLine(desc);
            }

            Console.WriteLine(new string('=', 90));
            Console.WriteLine();
        }
    }

    public static void ShowTimerInfo(string name)
    {
        lock (_lockObject)
        {
            if (!_timers.ContainsKey(name))
            {
                ConsoleManager.WriteLineColored($"Timer '{name}' not found",
                    ConsoleStyle.Colors.Error);
                return;
            }

            var info = _timers[name];

            ConsoleManager.WriteLineColored($"\nTimer Information: {name}",
                ConsoleStyle.Colors.Primary);
            Console.WriteLine(new string('=', 50));

            ConsoleManager.WriteColored("  Status:        ", ConsoleStyle.Colors.Primary);
            if (info.IsEnabled)
            {
                ConsoleManager.WriteLineColored("Enabled", ConsoleStyle.Colors.Success);
            }
            else
            {
                ConsoleManager.WriteLineColored("Disabled", ConsoleStyle.Colors.Warning);
            }

            ConsoleManager.WriteColored("  Type:          ", ConsoleStyle.Colors.Primary);
            Console.WriteLine(info.IsRecurring ? "Recurring" : "One-time");

            ConsoleManager.WriteColored("  Frequency:     ", ConsoleStyle.Colors.Primary);
            Console.WriteLine(FormatFrequency(info.Frequency));

            ConsoleManager.WriteColored("  Created:       ", ConsoleStyle.Colors.Primary);
            Console.WriteLine(info.CreatedAt);

            ConsoleManager.WriteColored("  Last Trigger:  ", ConsoleStyle.Colors.Primary);
            if (info.LastTriggered != System.DateTime.MinValue)
            {
                Console.WriteLine(info.LastTriggered);
            }
            else
            {
                Console.WriteLine("Never");
            }

            ConsoleManager.WriteColored("  Executions:    ", ConsoleStyle.Colors.Primary);
            Console.WriteLine(_timerExecutionCount[name]);

            ConsoleManager.WriteColored("  Description:   ", ConsoleStyle.Colors.Primary);
            Console.WriteLine(string.IsNullOrEmpty(info.Description) ? "-" : info.Description);

            Console.WriteLine(new string('=', 50));
            Console.WriteLine();
        }
    }

    private static string FormatFrequency(ulong nanoseconds)
    {
        if (nanoseconds >= 1000000000)
        {
            return $"{nanoseconds / 1000000000}s";
        }
        else if (nanoseconds >= 1000000)
        {
            return $"{nanoseconds / 1000000}ms";
        }
        else if (nanoseconds >= 1000)
        {
            return $"{nanoseconds / 1000}μs";
        }
        else
        {
            return $"{nanoseconds}ns";
        }
    }

    public static bool TimerExists(string name)
    {
        lock (_lockObject)
        {
            return _timers.ContainsKey(name);
        }
    }

    public static int GetTimerCount()
    {
        lock (_lockObject)
        {
            return _timers.Count;
        }
    }

    public static void Shutdown()
    {
        lock (_lockObject)
        {
            Console.WriteLine("[TimerManager] Shutting down timer system...");

            int count = _timers.Count;

            foreach (var timerInfo in _timers.Values)
            {
                try
                {
                    Cosmos.HAL.Global.PIT.UnregisterTimer(timerInfo.Timer.TimerID);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TimerManager] Error unregistering timer: {ex.Message}");
                }
            }

            _timers.Clear();
            _timerFlags.Clear();
            _timerExecutionCount.Clear();
            _isInitialized = false;

            Console.WriteLine($"[TimerManager] Shutdown complete. {count} timers stopped.");
            Log.WriteLog(LogPriority.Info, "TimerManager",
                $"Timer system shutdown - {count} timers stopped", "SYSTEM");
        }
    }
}