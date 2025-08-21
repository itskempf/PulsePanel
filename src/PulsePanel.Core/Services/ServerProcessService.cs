using System.Diagnostics;
using System.Text;
using PulsePanel.Core.Models;
using PulsePanel.Core.Services;
using System;
using System.IO;

namespace PulsePanel.Core.Services;

public class ServerProcessService {
    private readonly IProvenanceLogger _logger;
    public ServerProcessService(IProvenanceLogger logger) {
        _logger = logger;
    }

    private string ServersRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "servers"));

    public string GetServerDir(string id) => Path.Combine(ServersRoot, id);
    public string FilesDir(string id) => Path.Combine(GetServerDir(id), "files");
    public string ConfigDir(string id) => Path.Combine(GetServerDir(id), "config");
    public string LogsDir(string id) => Path.Combine(GetServerDir(id), "logs");

    public void EnsureLayout(string id) {
        Directory.CreateDirectory(FilesDir(id));
        Directory.CreateDirectory(ConfigDir(id));
        Directory.CreateDirectory(LogsDir(id));
    }

    public (Process? proc, Exception? error) Start(ServerEntry s, string execRelative, string args) {
        try {
            EnsureLayout(s.Id);
            var work = FilesDir(s.Id);
            var outLogPath = Path.Combine(LogsDir(s.Id), "server.out.log");
            var errLogPath = Path.Combine(LogsDir(s.Id), "server.err.log");
            var outLog = new StreamWriter(new FileStream(outLogPath, FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true };
            var errLog = new StreamWriter(new FileStream(errLogPath, FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true };

            string exec = ResolveExec(work, execRelative);
            var psi = new ProcessStartInfo {
                FileName = exec,
                Arguments = args,
                WorkingDirectory = work,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
            p.OutputDataReceived += (_, e) => { if (e.Data != null) outLog.WriteLine(e.Data); };
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) errLog.WriteLine(e.Data); };
            p.Exited += (_, __) => { try { outLog.Dispose(); errLog.Dispose(); } catch {} };

            if (!p.Start()) return (null, new Exception("Failed to start process."));
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            return (p, null);
        } catch (Exception ex) {
            return (null, ex);
        }
    }

    public bool Stop(int pid) {
        try {
            var p = Process.GetProcessById(pid);
            if (p.HasExited) return true;
            p.CloseMainWindow();
            if (!p.WaitForExit(3000)) p.Kill(true);
            return true;
        } catch {
            return false;
        }
    }

    private static string ResolveExec(string workingDir, string relativeExec) {
        var candidate = Path.Combine(workingDir, relativeExec.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (OperatingSystem.IsWindows() && !candidate.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
            if (File.Exists(candidate + ".exe")) candidate += ".exe";
        }
        return candidate;
    }
}