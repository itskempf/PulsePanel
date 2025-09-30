using System.IO;
using System.Text.Json;

namespace PulsePanel
{
    public class ServerMetrics
    {
        public DateTime Timestamp { get; set; }
        public string ServerId { get; set; } = "";
        public float CpuPercent { get; set; }
        public long RamBytes { get; set; }
        public ServerStatus Status { get; set; }
        public TimeSpan Uptime { get; set; }
    }

    public static class MetricsCollector
    {
        private static readonly string MetricsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PulsePanel", "metrics");
        private static readonly Timer _metricsTimer = new(CollectMetrics, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        private static readonly List<GameServer> _servers = new();

        static MetricsCollector()
        {
            Directory.CreateDirectory(MetricsPath);
        }

        public static void RegisterServer(GameServer server)
        {
            lock (_servers)
            {
                if (!_servers.Contains(server))
                    _servers.Add(server);
            }
        }

        public static void UnregisterServer(GameServer server)
        {
            lock (_servers)
            {
                _servers.Remove(server);
            }
        }

        private static void CollectMetrics(object? state)
        {
            try
            {
                var metrics = new List<ServerMetrics>();
                var now = DateTime.Now;

                lock (_servers)
                {
                    foreach (var server in _servers)
                    {
                        var metric = new ServerMetrics
                        {
                            Timestamp = now,
                            ServerId = server.Id,
                            Status = server.Status
                        };

                        if (server.ServerProcess != null && !server.ServerProcess.HasExited)
                        {
                            try
                            {
                                metric.CpuPercent = (float)server.ServerProcess.TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount / 10;
                                metric.RamBytes = server.ServerProcess.WorkingSet64;
                                metric.Uptime = DateTime.Now - server.ServerProcess.StartTime;
                            }
                            catch { }
                        }

                        metrics.Add(metric);
                    }
                }

                if (metrics.Any())
                {
                    var fileName = $"metrics_{now:yyyyMMdd}.json";
                    var filePath = Path.Combine(MetricsPath, fileName);
                    
                    var existingMetrics = new List<ServerMetrics>();
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            var json = File.ReadAllText(filePath);
                            existingMetrics = JsonSerializer.Deserialize<List<ServerMetrics>>(json) ?? new();
                        }
                        catch { }
                    }

                    existingMetrics.AddRange(metrics);
                    var updatedJson = JsonSerializer.Serialize(existingMetrics, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, updatedJson);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Metrics collection failed", ex);
            }
        }

        public static List<ServerMetrics> GetMetrics(string serverId, DateTime from, DateTime to)
        {
            var allMetrics = new List<ServerMetrics>();
            
            for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
            {
                var fileName = $"metrics_{date:yyyyMMdd}.json";
                var filePath = Path.Combine(MetricsPath, fileName);
                
                if (File.Exists(filePath))
                {
                    try
                    {
                        var json = File.ReadAllText(filePath);
                        var dayMetrics = JsonSerializer.Deserialize<List<ServerMetrics>>(json) ?? new();
                        allMetrics.AddRange(dayMetrics.Where(m => m.ServerId == serverId && m.Timestamp >= from && m.Timestamp <= to));
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning($"Failed to load metrics from {fileName}: {ex.Message}");
                    }
                }
            }

            return allMetrics.OrderBy(m => m.Timestamp).ToList();
        }

        public static void Dispose()
        {
            _metricsTimer?.Dispose();
        }
    }
}