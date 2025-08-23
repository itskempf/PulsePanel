using System;

namespace PulsePanel.Core.Events
{
    public class BlueprintInstallStartedEvent : DomainEvent
    {
        public override string Name => "Blueprint.InstallStarted";
        public string BlueprintId { get; }
        public string BlueprintName { get; }
        public string ServerId { get; }
        public string ServerName { get; }
        public override object Payload => new { BlueprintId, BlueprintName, ServerId, ServerName };

        public BlueprintInstallStartedEvent(string blueprintId, string blueprintName, string serverId, string serverName, 
            string correlationId = null, string causationId = null, string actorId = null) 
            : base(correlationId, causationId, actorId, "Blueprint")
        {
            BlueprintId = blueprintId;
            BlueprintName = blueprintName;
            ServerId = serverId;
            ServerName = serverName;
        }
    }

    public class BlueprintInstallFailedEvent : DomainEvent
    {
        public override string Name => "Blueprint.InstallFailed";
        public string BlueprintId { get; }
        public string BlueprintName { get; }
        public string ServerId { get; }
        public string ServerName { get; }
        public string Reason { get; }
        public object Details { get; }
        public override object Payload => new { BlueprintId, BlueprintName, ServerId, ServerName, Reason, Details };

        public BlueprintInstallFailedEvent(string blueprintId, string blueprintName, string serverId, string serverName, 
            string reason, object details = null, string correlationId = null, string causationId = null, string actorId = null) 
            : base(correlationId, causationId, actorId, "Blueprint")
        {
            BlueprintId = blueprintId;
            BlueprintName = blueprintName;
            ServerId = serverId;
            ServerName = serverName;
            Reason = reason;
            Details = details;
        }
    }

    public class BlueprintInstallSucceededEvent : DomainEvent
    {
        public override string Name => "Blueprint.InstallSucceeded";
        public string BlueprintId { get; }
        public string BlueprintName { get; }
        public string ServerId { get; }
        public string ServerName { get; }
        public override object Payload => new { BlueprintId, BlueprintName, ServerId, ServerName };

        public BlueprintInstallSucceededEvent(string blueprintId, string blueprintName, string serverId, string serverName, 
            string correlationId = null, string causationId = null, string actorId = null) 
            : base(correlationId, causationId, actorId, "Blueprint")
        {
            BlueprintId = blueprintId;
            BlueprintName = blueprintName;
            ServerId = serverId;
            ServerName = serverName;
        }
    }
}