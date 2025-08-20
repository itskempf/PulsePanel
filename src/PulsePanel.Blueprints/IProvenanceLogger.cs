using PulsePanel.Blueprints.Provenance;

namespace PulsePanel.Blueprints;

public interface IProvenanceLogger
{
    void Log(LogEntry entry);
}
