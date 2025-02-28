using System;
using System.Collections.Generic;
using Cosmos.HAL;
using TheSailOSProject.Logging;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Hardware.Timer
{
    public class TimerManager
    {
        private static bool _isInitialized = false;
        private static Dictionary<string, PIT.PITTimer> _timers = new Dictionary<string, PIT.PITTimer>();
        private static Dictionary<string, bool> _timerFlags = new Dictionary<string, bool>();
        
        public static void Initialize()
        {
            if (_isInitialized)
                return;

            try
            {
                ConsoleManager.WriteLineColored("[TimerManager] Initializing system timer...", ConsoleStyle.Colors.Primary);
                
                CreateTimer("system", 250000000, SystemTimerCallback, true);
                
                _isInitialized = true;
                Log.WriteLog(LogPriority.Info, "TimerManager", "Timer system initialized successfully", "SYSTEM");
                ConsoleManager.WriteLineColored("[TimerManager] Timer system initialized successfully", ConsoleStyle.Colors.Success);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogPriority.Error, "TimerManager", $"Timer initialization failed: {ex.Message}", "SYSTEM");
                ConsoleManager.WriteLineColored($"[TimerManager] Timer initialization failed: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }
        
        public static bool CreateTimer(string name, ulong frequency, Action callback, bool recurring)
        {
            try
            {
                if (_timers.ContainsKey(name))
                {
                    ConsoleManager.WriteLineColored($"[TimerManager] Timer '{name}' already exists", ConsoleStyle.Colors.Warning);
                    return false;
                }
                
                ConsoleManager.WriteLineColored($"[TimerManager] Creating timer '{name}'", ConsoleStyle.Colors.Primary);
                
                PIT.PITTimer timer = new PIT.PITTimer(callback, frequency, recurring);
                Cosmos.HAL.Global.PIT.RegisterTimer(timer);
                
                _timers.Add(name, timer);
                _timerFlags.Add(name, false);
                
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogPriority.Error, "TimerManager", $"Failed to create timer '{name}': {ex.Message}", "SYSTEM");
                ConsoleManager.WriteLineColored($"[TimerManager] Failed to create timer '{name}': {ex.Message}", ConsoleStyle.Colors.Error);
                return false;
            }
        }
        
        public static bool DestroyTimer(string name)
        {
            try
            {
                if (!_timers.ContainsKey(name))
                {
                    ConsoleManager.WriteLineColored($"[TimerManager] Timer '{name}' not found", ConsoleStyle.Colors.Warning);
                    return false;
                }
                
                var timer = _timers[name];
                Cosmos.HAL.Global.PIT.UnregisterTimer(timer.TimerID);
                
                _timers.Remove(name);
                _timerFlags.Remove(name);
                
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogPriority.Error, "TimerManager", $"Failed to destroy timer '{name}': {ex.Message}", "SYSTEM");
                ConsoleManager.WriteLineColored($"[TimerManager] Failed to destroy timer '{name}': {ex.Message}", ConsoleStyle.Colors.Error);
                return false;
            }
        }
        
        public static bool HasTimerTriggered(string name)
        {
            if (!_timerFlags.ContainsKey(name))
                return false;

            bool triggered = _timerFlags[name];
            _timerFlags[name] = false;
            return triggered;
        }
        
        public static void ListTimers()
        {
            ConsoleManager.WriteLineColored("=== Active Timers ===", ConsoleStyle.Colors.Primary);
            foreach (var timer in _timers)
            {
                ConsoleManager.WriteColored($"Timer '{timer.Key}': ", ConsoleStyle.Colors.Primary);
                ConsoleManager.WriteLineColored("Running", ConsoleStyle.Colors.Success);
            }
        }
        
        private static void SystemTimerCallback()
        {
            // Set the system timer flag
            _timerFlags["system"] = true;
            
            // This could also update system time, check for scheduled tasks, etc.
        }
        
        public static void Shutdown()
        {
            foreach (var timer in _timers.Values)
            {
                try
                {
                    Cosmos.HAL.Global.PIT.UnregisterTimer(timer.TimerID);
                }
                catch {}
            }
            
            _timers.Clear();
            _timerFlags.Clear();
            _isInitialized = false;
        }
    }
}