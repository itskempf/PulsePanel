namespace PulsePanel.Core
{
    public interface IProvenanceLogger
    {
        void Log(string eventName, object data);
    }
}
