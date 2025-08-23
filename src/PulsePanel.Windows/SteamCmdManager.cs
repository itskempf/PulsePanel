using PulsePanel.Blueprints.Provenance;
using PulsePanel.Core.Services;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace PulsePanel.Windows
{
    public class SteamCmdManager : ISteamCmdManager
    {
        private readonly IProvenanceLogger _logger;
        private readonly string _steamCmdDir;

        public SteamCmdManager(IProvenanceLogger logger)
        {
            _logger = logger;
            _steamCmdDir = Path.Combine(AppContext.BaseDirectory, "tools", "steamcmd");
        }

        public void SetupSteamCmd()
        {
            Directory.CreateDirectory(_steamCmdDir);
            var zipPath = Path.Combine(_steamCmdDir, "steamcmd.zip");

            using var client = new HttpClient();
            var data = client.GetByteArrayAsync("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip").Result;
            File.WriteAllBytes(zipPath, data);

            ZipFile.ExtractToDirectory(zipPath, _steamCmdDir, overwriteFiles: true);
            File.Delete(zipPath);

            _logger.Log(new LogEntry
            {
                Action = "SteamCmdSetup",
                Timestamp = DateTime.UtcNow,
                Metadata = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "Path", _steamCmdDir }
                }
            });
        }

        public bool VerifySteamCmd()
        {
            var exe = Path.Combine(_steamCmdDir, "steamcmd.exe");
            var exists = File.Exists(exe);

            _logger.Log(new LogEntry
            {
                Action = "SteamCmdVerified",
                Timestamp = DateTime.UtcNow,
                Metadata = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "Path", exe },
                    { "Exists", exists }
                }
            });

            return exists;
        }
    }
}
