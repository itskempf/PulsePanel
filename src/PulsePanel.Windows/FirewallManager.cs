using PulsePanel.Blueprints;
using PulsePanel.Blueprints.Provenance;
using System.Diagnostics;

namespace PulsePanel.Windows
{
    public class FirewallManager
    {
        private readonly IProvenanceLogger _logger;

        public FirewallManager(IProvenanceLogger logger)
        {
            _logger = logger;
        }

        public void AddFirewallRule(string ruleName, string protocol, string port)
        {
            string arguments = $"advfirewall firewall add rule name=\"{ruleName}\" dir=in action=allow protocol={protocol} localport={port}";
            Process.Start("netsh.exe", arguments);
            _logger.Log(new LogEntry
            {
                Action = "firewall-add",
                Results = new ResultsInfo { Status = "success" },
                Inputs = new InputsInfo { MetaPath = ruleName } // Using MetaPath to store the rule name
            });
        }

        public void RemoveFirewallRule(string ruleName)
        {
            string arguments = $"advfirewall firewall delete rule name=\"{ruleName}\"" ;
            Process.Start("netsh.exe", arguments);
            _logger.Log(new LogEntry
            {
                Action = "firewall-remove",
                Results = new ResultsInfo { Status = "success" },
                Inputs = new InputsInfo { MetaPath = ruleName } // Using MetaPath to store the rule name
            });
        }
    }
}
