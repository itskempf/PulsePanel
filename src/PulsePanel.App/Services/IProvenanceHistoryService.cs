using System;
using System.Collections.Generic;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public interface IProvenanceHistoryService
    {
        void SaveSession(ExecutionSession session);
        IEnumerable<ExecutionSession> GetAllSessions();
        ExecutionSession? GetSession(Guid id);
    }
}