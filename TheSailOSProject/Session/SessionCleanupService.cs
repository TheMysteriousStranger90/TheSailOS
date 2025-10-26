using System;
using TheSailOSProject.Processes;

namespace TheSailOSProject.Session;

public class SessionCleanupService : Process
{
    private const string TimerName = "session_cleanup_timer";
    private const ulong CleanupIntervalNs = 300000000000;
    private const int InactiveTimeoutMinutes = 30;
    private bool _isTimerRegistered = false;

    public SessionCleanupService() : base("SessionCleanupService", ProcessType.Service)
    {
    }

    public override void Start()
    {
        base.Start();
        
        if (!_isTimerRegistered)
        {
            bool created = TheSailOSProject.Hardware.Timer.TimerManager.CreateTimer(
                TimerName,
                CleanupIntervalNs,
                OnTimerTick,
                true,
                "Periodic session cleanup"
            );

            if (created)
            {
                _isTimerRegistered = true;
                Console.WriteLine("[SessionCleanupService] Timer registered successfully");
            }
        }
    }

    public override void Run()
    {
        if (_isTimerRegistered && 
            TheSailOSProject.Hardware.Timer.TimerManager.HasTimerTriggered(TimerName))
        {
            CleanupSessions();
        }
    }

    public override void Stop()
    {
        if (_isTimerRegistered)
        {
            TheSailOSProject.Hardware.Timer.TimerManager.DestroyTimer(TimerName);
            _isTimerRegistered = false;
            Console.WriteLine("[SessionCleanupService] Timer unregistered");
        }
        
        base.Stop();
    }

    private void OnTimerTick()
    {
    }

    private void CleanupSessions()
    {
        try
        {
            var timeout = System.TimeSpan.FromMinutes(InactiveTimeoutMinutes);
            SessionManager.CleanupInactiveSessions(timeout);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SessionCleanupService] Cleanup error: {ex.Message}");
        }
    }
}