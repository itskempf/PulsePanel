namespace PulsePanel.Core.Automation
{
    public interface IScheduledTask
    {
        string Name { get; }
        ITrigger Trigger { get; }
        void Execute(object context);
    }

    public interface ITrigger
    {
        TriggerType Type { get; }
        string Name { get; }
        bool ShouldRun(DateTime now);
    }
}
