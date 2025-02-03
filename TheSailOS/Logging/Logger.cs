using System;
using TheSailOS.FileSystemTheSail;

namespace TheSailOS.Logging;

public class Logger
{
    private FileWriter _fileWriter;
    private LogLevel _logLevel;

    public Logger(FileWriter fileWriter, LogLevel logLevel)
    {
        _fileWriter = fileWriter;
        _logLevel = logLevel;
    }

    public void LogInfo(string message)
    {
        if (_logLevel <= LogLevel.Info)
        {
            WriteLog("INFO", message);
        }
    }

    public void LogWarning(string message)
    {
        if (_logLevel <= LogLevel.Warning)
        {
            WriteLog("WARNING", message);
        }
    }

    public void LogError(string message)
    {
        if (_logLevel <= LogLevel.Error)
        {
            WriteLog("ERROR", message);
        }
    }

    private void WriteLog(string level, string message)
    {
        string logMessage = $"{DateTime.Now}: {level} - {message}";
        _fileWriter.WriteFile("path_to_your_log_file.log", logMessage);
    }
}