using System.IO;

namespace PulsePanel
{
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PulsePanel", "logs");
        private static readonly object LogLock = new();

        static Logger()
        {
            Directory.CreateDirectory(LogPath);
        }

        public static void LogError(string message, Exception? ex = null)
        {
            Log("ERROR", message, ex);
        }

        public static void LogWarning(string message)
        {
            Log("WARN", message);
        }

        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        private static void Log(string level, string message, Exception? ex = null)
        {
            try
            {
                lock (LogLock)
                {
                    var logFile = Path.Combine(LogPath, $"pulsepanel_{DateTime.Now:yyyyMMdd}.log");
                    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                    
                    if (ex != null)
                        logEntry += $"\nException: {ex}";
                    
                    File.AppendAllText(logFile, logEntry + Environment.NewLine);
                }
            }
            catch { }
        }
    }
}