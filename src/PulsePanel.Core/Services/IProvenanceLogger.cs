using PulsePanel.Blueprints.Provenance;

namespace PulsePanel.Core.Services
{
    public interface IProvenanceLogger
    {
        void Log(ProvenanceEvent evt);
    }
}
