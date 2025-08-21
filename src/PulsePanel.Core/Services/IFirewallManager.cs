namespace PulsePanel.Core.Services
{
    public interface IFirewallManager
    {
        void AddFirewallRule(string ruleName, string protocol, string port);
        void RemoveFirewallRule(string ruleName);
    }
}
