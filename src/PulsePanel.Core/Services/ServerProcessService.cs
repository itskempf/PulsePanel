using System.Collections.Concurrent;
using PulsePanel.Blueprints.Provenance;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services;

public class ServerProcessService
{
    private readonly ProvenanceLogger _logger;
    // Placeholder running-state tracker. In the future this can hold real process handles.
    private readonly ConcurrentDictionary<string, byte> _runningServers = new();

    public ServerProcessService(ProvenanceLogger logger)
    {
        _logger = logger;
    }

    private string ServersRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "servers"));
    public string GetServerDir(string id) => Path.Combine(ServersRoot, id);
    public string FilesDir(string id) => Path.Combine(GetServerDir(id), "files");
    public string ConfigDir(string id) => Path.Combine(GetServerDir(id), "config");
    public string LogsDir(string id) => Path.Combine(GetServerDir(id), "logs");

    public void EnsureLayout(string id)
    {
        Directory.CreateDirectory(FilesDir(id));
        Directory.CreateDirectory(ConfigDir(id));
        Directory.CreateDirectory(LogsDir(id));
    }

    public ServerStatus GetServerStatus(ServerEntry serverEntry)
    {
        return _runningServers.ContainsKey(serverEntry.Id) ? ServerStatus.Running : ServerStatus.Stopped;
    }

    public static string ResolveExec(string workingDir, string relativeExec)
    {
        var normalized = relativeExec.Replace("/", Path.DirectorySeparatorChar.ToString());
        var candidate = Path.Combine(workingDir, normalized);
        if (OperatingSystem.IsWindows() && !candidate.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            if (File.Exists(candidate + ".exe")) candidate += ".exe";
        }
        return candidate;
    }

    // Placeholder lifecycle operations — these just flip in-memory state for now
    public void Start(string id)
    {
        EnsureLayout(id);
        _runningServers[id] = 1;
    }

    public void Stop(string id)
    {
        _runningServers.TryRemove(id, out _);
    }
}