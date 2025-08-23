using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using PulsePanel.Core.Events;
using PulsePanel.Core.Models;
using PulsePanel.Core.Services;
using Xunit;

namespace PulsePanel.Tests.Services
{
    public class BlueprintInstallerTests
    {
        private readonly Mock<IProvenanceLogger> _provenanceLogger;
        private readonly Mock<PnccLChecker> _pnccLChecker;
        private readonly Mock<ServerProcessService> _serverProcessService;
        private readonly Mock<IEventBus> _eventBus;
        private readonly BlueprintInstaller _installer;

        public BlueprintInstallerTests()
        {
            _provenanceLogger = new Mock<IProvenanceLogger>();
            _pnccLChecker = new Mock<PnccLChecker>();
            _serverProcessService = new Mock<ServerProcessService>();
            _eventBus = new Mock<IEventBus>();

            _installer = new BlueprintInstaller(
                _provenanceLogger.Object,
                _pnccLChecker.Object,
                _serverProcessService.Object,
                _eventBus.Object
            );
        }

        [Fact]
        public async Task InstallBlueprint_SuccessfulInstall_PublishesCorrectEvents()
        {
            // Arrange
            var blueprint = new Blueprint { Id = "test-bp", Name = "Test Blueprint" };
            var server = new ServerEntry { Id = "srv-1", Name = "Test Server" };
            var inputs = new Dictionary<string, string>();

            _pnccLChecker.Setup(x => x.CheckBlueprint(It.IsAny<Blueprint>()))
                .ReturnsAsync(new PnccLValidationResult { IsValid = true });

            // Act
            var result = await _installer.InstallBlueprint(blueprint, server, inputs);

            // Assert
            Assert.True(result);
            
            _eventBus.Verify(x => x.Publish(It.Is<BlueprintInstallStartedEvent>(e => 
                e.BlueprintId == blueprint.Id && 
                e.ServerId == server.Id)), Times.Once);

            _eventBus.Verify(x => x.Publish(It.Is<BlueprintInstallSucceededEvent>(e => 
                e.BlueprintId == blueprint.Id && 
                e.ServerId == server.Id)), Times.Once);
        }

        [Fact]
        public async Task InstallBlueprint_PnccLCheckFails_PublishesFailureEvent()
        {
            // Arrange
            var blueprint = new Blueprint { Id = "test-bp", Name = "Test Blueprint" };
            var server = new ServerEntry { Id = "srv-1", Name = "Test Server" };
            var inputs = new Dictionary<string, string>();

            _pnccLChecker.Setup(x => x.CheckBlueprint(It.IsAny<Blueprint>()))
                .ReturnsAsync(new PnccLValidationResult { IsValid = false });

            // Act
            var result = await _installer.InstallBlueprint(blueprint, server, inputs);

            // Assert
            Assert.False(result);
            
            _eventBus.Verify(x => x.Publish(It.Is<BlueprintInstallStartedEvent>(e => 
                e.BlueprintId == blueprint.Id && 
                e.ServerId == server.Id)), Times.Once);

            _eventBus.Verify(x => x.Publish(It.Is<BlueprintInstallFailedEvent>(e => 
                e.BlueprintId == blueprint.Id && 
                e.ServerId == server.Id && 
                e.Reason.Contains("PNCCL compliance"))), Times.Once);
        }

        [Fact]
        public async Task InstallBlueprint_UnexpectedError_PublishesErrorEvent()
        {
            // Arrange
            var blueprint = new Blueprint { Id = "test-bp", Name = "Test Blueprint" };
            var server = new ServerEntry { Id = "srv-1", Name = "Test Server" };
            var inputs = new Dictionary<string, string>();

            _pnccLChecker.Setup(x => x.CheckBlueprint(It.IsAny<Blueprint>()))
                .ReturnsAsync(new PnccLValidationResult { IsValid = true });

            _serverProcessService.Setup(x => x.EnsureLayout(It.IsAny<string>()))
                .Throws(new Exception("Test error"));

            // Act
            var result = await _installer.InstallBlueprint(blueprint, server, inputs);

            // Assert
            Assert.False(result);
            
            _eventBus.Verify(x => x.Publish(It.Is<BlueprintInstallStartedEvent>(e => 
                e.BlueprintId == blueprint.Id && 
                e.ServerId == server.Id)), Times.Once);

            _eventBus.Verify(x => x.Publish(It.Is<BlueprintInstallFailedEvent>(e => 
                e.BlueprintId == blueprint.Id && 
                e.ServerId == server.Id && 
                e.Reason.Contains("Unexpected error"))), Times.Once);
        }
    }
}