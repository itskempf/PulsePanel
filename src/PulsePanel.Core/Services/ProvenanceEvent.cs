namespace PulsePanel.Core.Services
{
    public class ProvenanceEvent
    {
        // Core properties 
        public string? Action { get; set; }
        public DateTime Timestamp { get; set; }
        
        // Correlation and causal tracking
        public string? CorrelationId { get; set; }
        public string? CausationId { get; set; }  // ID of event that caused this one
        public string? EventId { get; set; }      // Unique ID of this event
        
        // Context metadata
        public string? ActorId { get; set; }      // User or system ID that triggered the event
        public string? ResourceId { get; set; }    // ID of resource being acted upon (e.g. serverId)
        public object? Metadata { get; set; }      // Additional context data
        
        // Optional category for grouping related events
        public string? Category { get; set; }
    }
}
