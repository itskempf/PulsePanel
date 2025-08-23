using System;

namespace PulsePanel.Core.Services
{
    public interface IProvenance
    {
        void Info(string action, object metadata = null, string correlationId = null, string actorId = null, string resourceId = null, string category = null);
        void Error(string action, string error, object metadata = null, string correlationId = null, string actorId = null, string resourceId = null, string category = null);
        IDisposable Begin(string action, object metadata = null, string correlationId = null, string actorId = null, string resourceId = null, string category = null);
    }
}
