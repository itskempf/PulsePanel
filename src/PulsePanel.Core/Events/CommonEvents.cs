using System;

namespace PulsePanel.Core.Events
{
    public class ServerStartedEvent : DomainEvent
    {
        public override string Name => "Server.Started";
        public string ServerId { get; }
        public override object Payload => new { ServerId };

        public ServerStartedEvent(string serverId, string correlationId = null, string causationId = null, string actorId = null) 
            : base(correlationId, causationId, actorId, "Server")
        {
            ServerId = serverId;
        }
    }

    public class ServerStoppedEvent : DomainEvent
    {
        public override string Name => "Server.Stopped";
        public string ServerId { get; }
        public override object Payload => new { ServerId };

        public ServerStoppedEvent(string serverId, string correlationId = null, string causationId = null, string actorId = null)
            : base(correlationId, causationId, actorId, "Server")
        {
            ServerId = serverId;
        }
    }

    public class ServerCrashedEvent : DomainEvent
    {
        public override string Name => "Server.Crashed";
        public string ServerId { get; }
        public string ErrorDetails { get; }
        public override object Payload => new { ServerId, ErrorDetails };

        public ServerCrashedEvent(string serverId, string errorDetails, string correlationId = null, string causationId = null, string actorId = null)
            : base(correlationId, causationId, actorId ?? "system", "Server")
        {
            ServerId = serverId;
            ErrorDetails = errorDetails;
        }
    }

    public class BlueprintInstalledEvent : DomainEvent
    {
        public override string Name => "Blueprint.Installed";
        public string BlueprintId { get; }
        public string ServerId { get; }
        public override object Payload => new { BlueprintId, ServerId };

        public BlueprintInstalledEvent(string blueprintId, string serverId, string correlationId = null, string causationId = null, string actorId = null)
            : base(correlationId, causationId, actorId, "Blueprint")
        {
            BlueprintId = blueprintId;
            ServerId = serverId;
        }
    }

    public class ConfigurationChangedEvent : DomainEvent
    {
        public override string Name => "Configuration.Changed";
        public string ServerId { get; }
        public string ConfigFile { get; }
        public override object Payload => new { ServerId, ConfigFile };

        public ConfigurationChangedEvent(string serverId, string configFile, string correlationId = null, string causationId = null, string actorId = null)
            : base(correlationId, causationId, actorId, "Configuration")
        {
            ServerId = serverId;
            ConfigFile = configFile;
        }
    }
}