namespace PulsePanel.Core.Services
{
    public class ProvenanceEvent
    {
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string CorrelationId { get; set; } // new
        public object Metadata { get; set; }
    }
}
