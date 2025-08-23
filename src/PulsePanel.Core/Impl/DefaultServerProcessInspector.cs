using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PulsePanel.Core.Services;

namespace PulsePanel.Core.Impl
{
    public sealed class DefaultServerProcessInspector : IServerProcessInspector
    {
        private readonly IServerService _servers;

        public DefaultServerProcessInspector(IServerService servers)
        {
            _servers = servers;
        }

        public async Task<IReadOnlyList<ServerProcessInfo>> GetRunningServersAsync()
        {
            var list = await _servers.GetServersAsync();
            var result = new List<ServerProcessInfo>();

            Process[] processes;
            try { processes = Process.GetProcesses(); }
            catch { return result; }

            foreach (var s in list)
            {
                var installDir = s.InstallDir;
                if (string.IsNullOrWhiteSpace(installDir)) continue;

                foreach (var p in processes)
                {
                    try
                    {
                        var exe = p.MainModule?.FileName;
                        if (exe is null) continue;
                        if (IsUnderDirectory(exe, installDir))
                        {
                            result.Add(new ServerProcessInfo
                            {
                                ServerId = s.Id,
                                ProcessId = p.Id,
                                ExecutablePath = exe
                            });
                        }
                    }
                    catch { }
                }
            }

            var dedup = result
                .GroupBy(r => r.ServerId)
                .Select(g =>
                {
                    var pick = g
                        .Select(info =>
                        {
                            try { return (info, Process.GetProcessById(info.ProcessId).StartTime); }
                            catch { return (info, DateTime.MinValue); }
                        })
                        .OrderByDescending(x => x.Item2)
                        .First().info;
                    return pick;
                })
                .ToList();

            return dedup;
        }

        private static bool IsUnderDirectory(string filePath, string dir)
        {
            try
            {
                var fullFile = Path.GetFullPath(filePath);
                var fullDir = Path.GetFullPath(dir).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                return fullFile.StartsWith(fullDir, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }
    }
}
