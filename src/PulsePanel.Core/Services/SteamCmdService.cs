using System.Runtime.InteropServices;
using System.IO.Compression;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System;
using PulsePanel.Core.Models;
using PulsePanel.Core.Services;

namespace PulsePanel.Core.Services;

public class SteamCmdService {
    private readonly string _toolsDir;
    private readonly IProvenanceLogger _logger;

    public SteamCmdService(IProvenanceLogger logger, SettingsService settingsService) {
        _logger = logger;
        var settings = settingsService.GetSettings();
        var steamCmdRelativePath = settings.SteamCmdPath ?? "steamcmd";
        var rootDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", ".."));
        _toolsDir = Path.Combine(rootDir, steamCmdRelativePath);
        Directory.CreateDirectory(_toolsDir);
    }

    public string SteamCmdPath {
        get {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return Path.Combine(_toolsDir, "steamcmd.exe");
            return Path.Combine(_toolsDir, "steamcmd.sh");
        }
    }

    public async Task EnsurePresentAsync(CancellationToken ct = default) {
        if (File.Exists(SteamCmdPath)) return;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            var zip = Path.Combine(_toolsDir, "steamcmd.zip");
            await DownloadAsync("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip", zip, ct);
            ZipFile.ExtractToDirectory(zip, _toolsDir, true);
            File.Delete(zip);
        } else {
            var tgz = Path.Combine(_toolsDir, "steamcmd_linux.tar.gz");
            await DownloadAsync("https://steamcdn-a.akamaihd.net/client/installer/steamcmd_linux.tar.gz", tgz, ct);
            using var fs = new FileStream(tgz, FileMode.Open, FileAccess.Read);
            using var gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress);
            using var tar = new TarReader(gz);
            tar.ExtractToDirectory(_toolsDir);
            File.Delete(tgz);
            var sh = SteamCmdPath;
            if (File.Exists(sh)) {
                var perm = new FileInfo(sh);
                try { UnixChmod(sh, "755"); } catch { }
            }
        }
    }

    public async Task<int> AppUpdateAsync(int appId, string installDir, bool validate = true, CancellationToken ct = default) {
        await EnsurePresentAsync(ct);
        Directory.CreateDirectory(installDir);
        string args = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? $"+login anonymous +force_install_dir \"{installDir}\" +app_update {appId} {(validate ? "validate" : "")} +quit"
            : $"+login anonymous +force_install_dir \"{installDir}\" +app_update {appId} {(validate ? "validate" : "")} +quit";

        var psi = new ProcessStartInfo {
            FileName = SteamCmdPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _toolsDir
        };
        using var p = Process.Start(psi)!;
        await Task.WhenAll(
            Task.Run(async () => { while (!p.HasExited) { await p.StandardOutput.ReadLineAsync(ct); } }),
            Task.Run(async () => { while (!p.HasExited) { await p.StandardError.ReadLineAsync(ct); } })
        );
        await p.WaitForExitAsync(ct);
        return p.ExitCode;
    }

    private static async Task DownloadAsync(string url, string path, CancellationToken ct) {
        using var http = new HttpClient();
        using var resp = await http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();
        await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        await resp.Content.CopyToAsync(fs, ct);
    }

    private static void UnixChmod(string path, string mode) {
        try { System.Diagnostics.Process.Start("chmod", $"{mode} \"{path}\""); } catch {}
    }
}

public sealed class TarReader : IDisposable {
    private readonly Stream _stream;
    public TarReader(Stream stream) => _stream = stream;
    public void ExtractToDirectory(string directory) {
        Directory.CreateDirectory(directory);
        Span<byte> header = stackalloc byte[512];
        while (true) {
            int read = _stream.Read(header);
            if (read < 512) break;
            bool isEnd = true;
            for (int i = 0; i < 512; i++) if (header[i] != 0) { isEnd = false; break; }
            if (isEnd) break;

            string name = GetString(header.Slice(0,100)).Trim('\0');
            string sizeOct = GetString(header.Slice(124,12)).Trim('\0').Trim();
            long size = Convert.ToInt64(string.IsNullOrWhiteSpace(sizeOct) ? "0" : sizeOct, 8);

            string target = Path.Combine(directory, name);
            string? dir = Path.GetDirectoryName(target);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            if (header[156] == (byte)'5') {
                Directory.CreateDirectory(target);
            } else if (header[156] == (byte)'0' || header[156] == 0) {
                using var fs = new FileStream(target, FileMode.Create, FileAccess.Write);
                CopyN(_stream, fs, size);
                long rem = size % 512;
                if (rem != 0) _stream.Position += (512 - rem);
            } else {
                _stream.Position += size;
                long rem = size % 512;
                if (rem != 0) _stream.Position += (512 - rem);
            }
        }
    }
    private static void CopyN(Stream src, Stream dst, long n) {
        byte[] buf = new byte[8192];
        long left = n;
        while (left > 0) {
            int toRead = (int)Math.Min(buf.Length, left);
            int read = src.Read(buf, 0, toRead);
            if (read <= 0) break;
            dst.Write(buf, 0, read);
            left -= read;
        }
    }
    private static string GetString(Span<byte> s) => System.Text.Encoding.ASCII.GetString(s);
    public void Dispose() { }
}
