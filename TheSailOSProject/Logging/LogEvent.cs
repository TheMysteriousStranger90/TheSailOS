using System;
using Cosmos.HAL;

namespace TheSailOSProject.Logging
{
    public class LogEvent
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        
        public int Priority { get; private set; }
        public string Source { get; private set; }
        public string Message { get; private set; }
        public string Username { get; private set; }

        public LogEvent(int priority, string source, string message, string username = "SYSTEM")
        {
            Year = RTC.Year;
            Month = RTC.Month;
            Day = RTC.DayOfTheMonth;
            Hour = RTC.Hour;
            Minute = RTC.Minute;
            Second = RTC.Second;
            
            Priority = priority;
            Source = source;
            Message = message;
            Username = username;
        }

        public override string ToString()
        {
            string date = $"{Day:D2}-{Month:D2}-{Year:D4}";
            string time = $"{Hour:D2}:{Minute:D2}:{Second:D2}";
            string timestamp = $"{date} {time}";
            
            return $"[{timestamp}] [{LogPriority.ToString(Priority)}] [{Source}] [{Username}] {Message}";
        }
        
        public static bool TryParse(string logLine, out LogEvent logEvent)
        {
            logEvent = null;
            
            try
            {
                int timestampEnd = logLine.IndexOf("]");
                int priorityEnd = logLine.IndexOf("]", timestampEnd + 1);
                int sourceEnd = logLine.IndexOf("]", priorityEnd + 1);
                int usernameEnd = logLine.IndexOf("]", sourceEnd + 1);
                
                if (timestampEnd < 0 || priorityEnd < 0 || sourceEnd < 0 || usernameEnd < 0)
                    return false;
                
                string timestampStr = logLine.Substring(1, timestampEnd - 1);
                string priorityStr = logLine.Substring(timestampEnd + 3, priorityEnd - timestampEnd - 3);
                string sourceStr = logLine.Substring(priorityEnd + 3, sourceEnd - priorityEnd - 3);
                string usernameStr = logLine.Substring(sourceEnd + 3, usernameEnd - sourceEnd - 3);
                string message = logLine.Substring(usernameEnd + 2);
                
                int day = int.Parse(timestampStr.Substring(0, 2));
                int month = int.Parse(timestampStr.Substring(3, 2));
                int year = int.Parse(timestampStr.Substring(6, 4));
                int hour = int.Parse(timestampStr.Substring(11, 2));
                int minute = int.Parse(timestampStr.Substring(14, 2));
                int second = int.Parse(timestampStr.Substring(17, 2));
                
                int priority = LogPriority.FromString(priorityStr);
                
                logEvent = new LogEvent(priority, sourceStr, message, usernameStr);
                logEvent.Year = year;
                logEvent.Month = month;
                logEvent.Day = day;
                logEvent.Hour = hour;
                logEvent.Minute = minute;
                logEvent.Second = second;
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}