namespace PulsePanel.Core.Events;

public interface IEvent
{
    string Name { get; }
    Guid CorrelationId { get; }
    Guid? CausationId { get; }
    DateTimeOffset Timestamp { get; }
    object? Payload { get; }
}
