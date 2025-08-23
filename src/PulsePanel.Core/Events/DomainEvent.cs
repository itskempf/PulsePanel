using System;

namespace PulsePanel.Core.Events
{
    /// <summary>
    /// Base class for all domain events in PulsePanel, integrating with the provenance system
    /// </summary>
    public abstract class DomainEvent
    {
        /// <summary>
        /// Unique identifier for this event instance
        /// </summary>
        public string EventId { get; } = Guid.NewGuid().ToString("n");

        /// <summary>
        /// The name of the event (e.g., "Server.Started", "Blueprint.Installed")
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// When the event occurred
        /// </summary>
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// ID linking related events in a flow
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// ID of the event that caused this one
        /// </summary>
        public string CausationId { get; set; }

        /// <summary>
        /// ID of user or system that triggered the event
        /// </summary>
        public string ActorId { get; set; }

        /// <summary>
        /// Category for grouping related events
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Event-specific data
        /// </summary>
        public virtual object Payload { get; } = null;

        protected DomainEvent(string correlationId = null, string causationId = null, string actorId = null, string category = null)
        {
            CorrelationId = correlationId ?? Guid.NewGuid().ToString("n");
            CausationId = causationId;
            ActorId = actorId ?? Environment.UserName;
            Category = category;
        }
    }
}