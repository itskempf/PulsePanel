using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using PulsePanel.Core.Services;

namespace PulsePanel.Core.Events
{
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent evt) where TEvent : DomainEvent;
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : DomainEvent;
        void Subscribe(string eventName, Action<DomainEvent> handler);
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : DomainEvent;
    }

    public sealed class EventBus : IEventBus
    {
        private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
        private readonly ConcurrentDictionary<string, List<Action<DomainEvent>>> _dynamicHandlers = new();
        private readonly IProvenanceLogger _provenance;

        public EventBus(IProvenanceLogger provenance)
        {
            _provenance = provenance;
        }

        public void Publish<TEvent>(TEvent evt) where TEvent : DomainEvent
        {
            // Log to provenance first
            _provenance.Log(new ProvenanceEvent
            {
                Action = evt.Name,
                EventId = evt.EventId,
                CorrelationId = evt.CorrelationId,
                CausationId = evt.CausationId,
                ActorId = evt.ActorId,
                Category = evt.Category,
                Timestamp = evt.Timestamp.UtcDateTime,
                Metadata = evt.Payload
            });

            // Notify typed handlers
            if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
            {
                foreach (var handler in handlers.ToArray())
                {
                    try
                    {
                        ((Action<TEvent>)handler)(evt);
                    }
                    catch (Exception ex)
                    {
                        // Log handler errors to provenance
                        _provenance.Log(new ProvenanceEvent
                        {
                            Action = "EventBus.HandlerError",
                            EventId = Guid.NewGuid().ToString("n"),
                            CorrelationId = evt.CorrelationId,
                            CausationId = evt.EventId,
                            Category = "EventBus",
                            Timestamp = DateTime.UtcNow,
                            Metadata = new { HandlerType = handler.GetType().Name, Error = ex.ToString() }
                        });
                    }
                }
            }

            // Notify dynamic handlers
            if (_dynamicHandlers.TryGetValue(evt.Name, out var dynamicHandlers))
            {
                foreach (var handler in dynamicHandlers.ToArray())
                {
                    try
                    {
                        handler(evt);
                    }
                    catch (Exception ex)
                    {
                        _provenance.Log(new ProvenanceEvent
                        {
                            Action = "EventBus.HandlerError",
                            EventId = Guid.NewGuid().ToString("n"),
                            CorrelationId = evt.CorrelationId,
                            CausationId = evt.EventId,
                            Category = "EventBus",
                            Timestamp = DateTime.UtcNow,
                            Metadata = new { HandlerType = "Dynamic", Error = ex.ToString() }
                        });
                    }
                }
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : DomainEvent
        {
            var handlers = _handlers.GetOrAdd(typeof(TEvent), _ => new List<Delegate>());
            lock (handlers)
            {
                handlers.Add(handler);
            }
        }

        public void Subscribe(string eventName, Action<DomainEvent> handler)
        {
            var handlers = _dynamicHandlers.GetOrAdd(eventName, _ => new List<Action<DomainEvent>>());
            lock (handlers)
            {
                handlers.Add(handler);
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : DomainEvent
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
            {
                lock (handlers)
                {
                    handlers.Remove(handler);
                }
            }
        }
    }
}