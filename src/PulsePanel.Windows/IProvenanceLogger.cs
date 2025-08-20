using PulsePanel.Blueprints.Provenance;

namespace PulsePanel.Windows;

public interface IProvenanceLogger
{
    void Log(LogEntry entry);
}
