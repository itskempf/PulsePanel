using System;

namespace PulsePanel.Core.Services
{
    public interface IProvenance
    {
        void Info(string action, object metadata = null, string correlationId = null);
        void Error(string action, string error, object metadata = null, string correlationId = null);
        IDisposable Begin(string action, object metadata = null, string correlationId = null);
    }
}
