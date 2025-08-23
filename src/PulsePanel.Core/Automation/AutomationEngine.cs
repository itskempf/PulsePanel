using System;
using System.Collections.Generic;
using System.Linq;

namespace PulsePanel.Core.Automation
{
    // PulsePanel.Core/Automation/AutomationEngine.cs
    // Purpose: Schedule tasks on time or event triggers
    // Twist: Event triggers can be extended via blueprints (e.g., "onPlayerJoin")

    public class AutomationEngine
    {
        private readonly List<IScheduledTask> _tasks = new();
        private readonly IProvenanceLogger _logger;

        public AutomationEngine(IProvenanceLogger logger)
        {
            _logger = logger;
        }

        public void ScheduleTask(IScheduledTask task)
        {
            _tasks.Add(task);
            _logger.Log("automation.schedule", new { task.Name, task.Trigger });
        }

        public void TriggerEvent(string eventName, object context)
        {
            foreach (var task in _tasks.Where(t => t.Trigger.Type == TriggerType.Event && t.Trigger.Name == eventName))
            {
                task.Execute(context);
                _logger.Log("automation.execute", new { task.Name, eventName });
            }
        }

        public void Tick(DateTime now)
        {
            foreach (var task in _tasks.Where(t => t.Trigger.Type == TriggerType.Time && t.Trigger.ShouldRun(now)))
            {
                task.Execute(null);
                _logger.Log("automation.execute", new { task.Name, now });
            }
        }
    }
}
