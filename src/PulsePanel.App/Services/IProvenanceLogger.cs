using System;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public interface IProvenanceLogger
    {
        Task InfoAsync(string message, Guid? sessionId = null);
        Task WarningAsync(string message, Guid? sessionId = null);
        Task ErrorAsync(string message, Guid? sessionId = null);
    }
}