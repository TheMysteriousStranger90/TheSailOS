using System;
using System.Collections.Generic;
using System.Linq;
using TheSailOSProject.Users;

namespace TheSailOSProject.Session;

public static class SessionManager
{
    private static Dictionary<string, Session> _activeSessions =
        new Dictionary<string, Session>(StringComparer.Ordinal);

    private static readonly object _sessionLock = new object();
    private static int _sessionCounter;
    private static int _totalSessionsCreated;

    private const int MAX_SESSIONS_PER_USER = 3;
    private const int MAX_TOTAL_SESSIONS = 50;
    private const int IDLE_TIMEOUT_MINUTES = 15;
    private const int SESSION_CLEANUP_THRESHOLD = 10;

    public static Session StartSession(User user)
    {
        if (user == null)
            return null;

        lock (_sessionLock)
        {
            int userSessionCount = 0;
            foreach (var sess in _activeSessions.Values)
            {
                if (sess.User.Username == user.Username && sess.Status == SessionStatus.Active)
                {
                    userSessionCount++;
                }
            }

            if (userSessionCount >= MAX_SESSIONS_PER_USER)
            {
                Console.WriteLine("[SessionManager] User session limit reached, terminating oldest");
                TerminateOldestUserSession(user.Username);
            }

            if (_activeSessions.Count >= MAX_TOTAL_SESSIONS)
            {
                Console.WriteLine("[SessionManager] Global session limit reached, cleanup initiated");
                CleanupInactiveSessions(System.TimeSpan.FromMinutes(5));
            }

            var timestamp = System.DateTime.Now.Ticks;
            var sessionId = user.Username + "-" + timestamp + "-" + (++_sessionCounter);

            var session = new Session(sessionId, user, System.DateTime.Now);
            _activeSessions.Add(sessionId, session);
            _totalSessionsCreated++;

            Console.WriteLine("[SessionManager] Session created: " + sessionId + " (Total active: " +
                              _activeSessions.Count + ")");
            return session;
        }
    }

    public static Session GetSession(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            return null;

        lock (_sessionLock)
        {
            if (_activeSessions.ContainsKey(sessionId))
            {
                return _activeSessions[sessionId];
            }

            return null;
        }
    }

    public static void EndSession(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            return;

        lock (_sessionLock)
        {
            if (_activeSessions.ContainsKey(sessionId))
            {
                var session = _activeSessions[sessionId];
                session.MarkAsTerminated();

                var duration = session.GetSessionDuration();
                Console.WriteLine("[SessionManager] Session ended: " + sessionId);
                Console.WriteLine("  Duration: " + session.GetFormattedDuration());
                Console.WriteLine("  Commands: " + session.CommandsExecuted);

                _activeSessions.Remove(sessionId);
            }
        }
    }

    public static void UpdateSessionActivity(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            return;

        lock (_sessionLock)
        {
            Session session = GetSession(sessionId);
            if (session != null)
            {
                session.UpdateActivity();

                CheckIdleSessions();
            }
        }
    }

    public static void IncrementSessionCommand(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
            return;

        lock (_sessionLock)
        {
            Session session = GetSession(sessionId);
            if (session != null)
            {
                session.IncrementCommandCount();
            }
        }
    }

    public static List<Session> GetAllSessions()
    {
        lock (_sessionLock)
        {
            return _activeSessions.Values.ToList();
        }
    }

    public static List<Session> GetUserSessions(string username)
    {
        if (string.IsNullOrEmpty(username))
            return new List<Session>();

        lock (_sessionLock)
        {
            var result = new List<Session>();
            foreach (var session in _activeSessions.Values)
            {
                if (session.User.Username.ToLower() == username.ToLower())
                {
                    result.Add(session);
                }
            }

            return result;
        }
    }

    public static void CleanupInactiveSessions(System.TimeSpan maxInactiveTime)
    {
        lock (_sessionLock)
        {
            var cutoff = System.DateTime.Now - maxInactiveTime;
            var toRemove = new List<Session>();

            foreach (var session in _activeSessions.Values)
            {
                if (session.LastActivity < cutoff && session.Status != SessionStatus.Terminated)
                {
                    toRemove.Add(session);
                }
            }

            if (toRemove.Count > 0)
            {
                Console.WriteLine("[SessionManager] Cleaning up " + toRemove.Count + " inactive sessions");

                foreach (var session in toRemove)
                {
                    session.MarkAsTerminated();
                    Console.WriteLine("  Terminated: " + session.SessionId + " (Idle: " +
                                      session.GetIdleTime().TotalMinutes.ToString("F1") + "m)");
                    _activeSessions.Remove(session.SessionId);
                }

                if (toRemove.Count > SESSION_CLEANUP_THRESHOLD)
                {
                    Cosmos.Core.Memory.Heap.Collect();
                }
            }
        }
    }

    private static void CheckIdleSessions()
    {
        var idleThreshold = System.TimeSpan.FromMinutes(IDLE_TIMEOUT_MINUTES);

        foreach (var session in _activeSessions.Values)
        {
            if (session.Status == SessionStatus.Active)
            {
                var idleTime = session.GetIdleTime();
                if (idleTime > idleThreshold)
                {
                    session.MarkAsIdle();
                    Console.WriteLine("[SessionManager] Session marked as idle: " + session.SessionId);
                }
            }
        }
    }

    private static void TerminateOldestUserSession(string username)
    {
        Session oldestSession = null;
        System.DateTime oldestTime = System.DateTime.MaxValue;

        foreach (var session in _activeSessions.Values)
        {
            if (session.User.Username == username && session.StartTime < oldestTime)
            {
                oldestTime = session.StartTime;
                oldestSession = session;
            }
        }

        if (oldestSession != null)
        {
            EndSession(oldestSession.SessionId);
        }
    }

    public static void TerminateAllUserSessions(string username)
    {
        if (string.IsNullOrEmpty(username))
            return;

        lock (_sessionLock)
        {
            var userSessions = GetUserSessions(username);
            foreach (var session in userSessions)
            {
                EndSession(session.SessionId);
            }

            Console.WriteLine("[SessionManager] Terminated " + userSessions.Count + " sessions for user: " +
                              username);
        }
    }

    public static SessionStatistics GetStatistics()
    {
        lock (_sessionLock)
        {
            int activeCount = 0;
            int idleCount = 0;
            long totalCommands = 0;
            long totalDurationTicks = 0;

            foreach (var session in _activeSessions.Values)
            {
                if (session.Status == SessionStatus.Active)
                    activeCount++;
                else if (session.Status == SessionStatus.Idle)
                    idleCount++;

                totalCommands += session.CommandsExecuted;
                totalDurationTicks += session.GetSessionDuration().Ticks;
            }

            var stats = new SessionStatistics
            {
                TotalActive = activeCount,
                TotalIdle = idleCount,
                TotalCreated = _totalSessionsCreated,
                TotalCommands = totalCommands,
                AverageDuration = _activeSessions.Count > 0
                    ? System.TimeSpan.FromTicks(totalDurationTicks / _activeSessions.Count)
                    : System.TimeSpan.Zero
            };

            return stats;
        }
    }

    public class SessionStatistics
    {
        public int TotalActive { get; set; }
        public int TotalIdle { get; set; }
        public int TotalCreated { get; set; }
        public long TotalCommands { get; set; }
        public System.TimeSpan AverageDuration { get; set; }

        public override string ToString()
        {
            return "Active: " + TotalActive + ", Idle: " + TotalIdle +
                   ", Created: " + TotalCreated + ", Avg Duration: " +
                   AverageDuration.TotalMinutes.ToString("F1") + "m";
        }
    }
}