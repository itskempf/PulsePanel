
using System;

namespace PulsePanel.Core.Models
{
    public class ScheduledTask
    {
        public Guid Id { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public Guid TargetServerId { get; set; }
        public string CronSchedule { get; set; } = "* * * * *";
        public bool IsEnabled { get; set; } = true;
    }
}
