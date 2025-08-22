using PulsePanel.Blueprints.Provenance;
using PulsePanel.Core.Services;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PulsePanel.Core.Services;

public class ProvenanceLogger
{
    private readonly string _logFilePath;
    private readonly long _maxLogSize;
    private readonly int _maxLogFiles;
    private static readonly object FileLock = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public ProvenanceLogger(string logFilePath, long maxLogSize = 10 * 1024 * 1024, int maxLogFiles = 10)
    {
        _logFilePath = logFilePath;
        _maxLogSize = maxLogSize;
        _maxLogFiles = maxLogFiles;

        var logDir = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrEmpty(logDir))
        {
            Directory.CreateDirectory(logDir);
        }
    }

    public async Task LogAsync(LogEntry entry)
    {
        var json = JsonSerializer.Serialize(entry, JsonOptions);

        // Use Task.Run to offload file I/O to a thread pool thread
        await Task.Run(() =>
        {
            lock (FileLock)
            {
                try
                {
                    RotateIfNeeded();
                    File.AppendAllText(_logFilePath, json + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    // In a real app, this should go to a fallback logger (e.g., console error)
                    Console.Error.WriteLine($"Failed to write to provenance log: {ex.Message}");
                }
            }
        });
    }

    private void RotateIfNeeded()
    {
        var fileInfo = new FileInfo(_logFilePath);
        if (!fileInfo.Exists || fileInfo.Length < _maxLogSize)
        {
            return;
        }

        var logDir = Path.GetDirectoryName(_logFilePath) ?? ".";
        var logFileName = Path.GetFileNameWithoutExtension(_logFilePath);
        var logExtension = Path.GetExtension(_logFilePath);

        // Delete the oldest backup
        var oldestLog = Path.Combine(logDir, $"{logFileName}.{_maxLogFiles}{logExtension}");
        if (File.Exists(oldestLog))
        {
            File.Delete(oldestLog);
        }

        // Shift existing backups
        for (int i = _maxLogFiles - 1; i > 0; i--)
        {
            var source = Path.Combine(logDir, $"{logFileName}.{i}{logExtension}");
            var dest = Path.Combine(logDir, $"{logFileName}.{i + 1}{logExtension}");
            if (File.Exists(source))
            {
                File.Move(source, dest);
            }
        }

        // Rename the current log file to the first backup
        var newBackupPath = Path.Combine(logDir, $"{logFileName}.1{logExtension}");
        File.Move(_logFilePath, newBackupPath);
    }
}
