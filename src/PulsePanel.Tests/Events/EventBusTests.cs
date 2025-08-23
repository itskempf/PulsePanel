using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using PulsePanel.Core.Events;
using PulsePanel.Core.Services;
using Xunit;

namespace PulsePanel.Tests.Events
{
    public class EventBusTests
    {
        private readonly Mock<IProvenanceLogger> _provenance;
        private readonly EventBus _bus;

        public EventBusTests()
        {
            _provenance = new Mock<IProvenanceLogger>();
            _bus = new EventBus(_provenance.Object);
        }

        [Fact]
        public void PublishEvent_InvokesHandler()
        {
            // Arrange
            var called = false;
            var testEvent = new ServerStartedEvent("test-server");
            _bus.Subscribe<ServerStartedEvent>(evt => called = true);

            // Act
            _bus.Publish(testEvent);

            // Assert
            Assert.True(called);
            _provenance.Verify(p => p.Log(It.IsAny<ProvenanceEvent>()), Times.Once);
        }

        [Fact]
        public void PublishEvent_LogsToProvenance()
        {
            // Arrange
            var testEvent = new ServerStartedEvent("test-server");

            // Act
            _bus.Publish(testEvent);

            // Assert
            _provenance.Verify(p => p.Log(It.Is<ProvenanceEvent>(e => 
                e.Action == testEvent.Name && 
                e.CorrelationId == testEvent.CorrelationId &&
                e.Category == "Server")), Times.Once);
        }

        [Fact]
        public void Subscribe_DynamicHandler_InvokedForMatchingEvent()
        {
            // Arrange
            var called = false;
            var testEvent = new ServerStartedEvent("test-server");
            _bus.Subscribe("Server.Started", evt => called = true);

            // Act
            _bus.Publish(testEvent);

            // Assert
            Assert.True(called);
            _provenance.Verify(p => p.Log(It.IsAny<ProvenanceEvent>()), Times.Once);
        }

        [Fact]
        public void HandlerError_LogsToProvenance()
        {
            // Arrange
            var testEvent = new ServerStartedEvent("test-server");
            _bus.Subscribe<ServerStartedEvent>(_ => throw new Exception("Test error"));

            // Act
            _bus.Publish(testEvent);

            // Assert
            _provenance.Verify(p => p.Log(It.Is<ProvenanceEvent>(e => 
                e.Action == "EventBus.HandlerError" && 
                e.Category == "EventBus")), Times.Once);
            _provenance.Verify(p => p.Log(It.Is<ProvenanceEvent>(e =>
                e.Action == testEvent.Name)), Times.Once);
        }
    }
}