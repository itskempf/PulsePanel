using System.Net.Mail;

namespace PulsePanel
{
    public enum AlertType
    {
        ServerCrash,
        HighCpuUsage,
        HighMemoryUsage,
        ServerOffline,
        UpdateAvailable
    }

    public class AlertRule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ServerId { get; set; } = "";
        public AlertType Type { get; set; }
        public double Threshold { get; set; }
        public bool EmailEnabled { get; set; }
        public bool ToastEnabled { get; set; } = true;
        public string EmailAddress { get; set; } = "";
        public DateTime LastTriggered { get; set; }
        public TimeSpan CooldownPeriod { get; set; } = TimeSpan.FromMinutes(15);
    }

    public static class AlertSystem
    {
        private static readonly List<AlertRule> _rules = new();
        private static readonly Dictionary<string, DateTime> _lastAlerts = new();

        public static void AddRule(AlertRule rule)
        {
            _rules.Add(rule);
        }

        public static void RemoveRule(string ruleId)
        {
            _rules.RemoveAll(r => r.Id == ruleId);
        }

        public static List<AlertRule> GetRules()
        {
            return _rules.ToList();
        }

        public static void CheckAlerts(GameServer server, float cpuPercent, long ramBytes)
        {
            foreach (var rule in _rules.Where(r => r.ServerId == server.Id))
            {
                var shouldAlert = false;
                var alertMessage = "";

                switch (rule.Type)
                {
                    case AlertType.ServerCrash:
                        if (server.Status == ServerStatus.Crashed)
                        {
                            shouldAlert = true;
                            alertMessage = $"Server {server.Name} has crashed";
                        }
                        break;

                    case AlertType.HighCpuUsage:
                        if (cpuPercent > rule.Threshold)
                        {
                            shouldAlert = true;
                            alertMessage = $"Server {server.Name} CPU usage is {cpuPercent:F1}% (threshold: {rule.Threshold}%)";
                        }
                        break;

                    case AlertType.HighMemoryUsage:
                        var ramMB = ramBytes / (1024 * 1024);
                        if (ramMB > rule.Threshold)
                        {
                            shouldAlert = true;
                            alertMessage = $"Server {server.Name} memory usage is {ramMB} MB (threshold: {rule.Threshold} MB)";
                        }
                        break;

                    case AlertType.ServerOffline:
                        if (server.Status == ServerStatus.Stopped || server.Status == ServerStatus.Crashed)
                        {
                            shouldAlert = true;
                            alertMessage = $"Server {server.Name} is offline";
                        }
                        break;
                }

                if (shouldAlert && CanTriggerAlert(rule))
                {
                    TriggerAlert(rule, alertMessage);
                }
            }
        }

        private static bool CanTriggerAlert(AlertRule rule)
        {
            return DateTime.Now - rule.LastTriggered > rule.CooldownPeriod;
        }

        private static void TriggerAlert(AlertRule rule, string message)
        {
            rule.LastTriggered = DateTime.Now;

            if (rule.ToastEnabled)
            {
                new ToastNotification("Server Alert", message, true);
            }

            if (rule.EmailEnabled && !string.IsNullOrEmpty(rule.EmailAddress))
            {
                Task.Run(() => SendEmailAlert(rule.EmailAddress, "PulsePanel Alert", message));
            }

            Logger.LogWarning($"Alert triggered: {message}");
        }

        private static Task SendEmailAlert(string emailAddress, string subject, string message)
        {
            return Task.Run(() =>
            {
                try
                {
                    // Basic email implementation - would need SMTP configuration
                    Logger.LogInfo($"Email alert would be sent to {emailAddress}: {message}");
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to send email alert", ex);
                }
            });
        }
    }
}