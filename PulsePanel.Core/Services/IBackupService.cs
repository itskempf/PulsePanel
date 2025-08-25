
using System.Collections.Generic;
using System.Threading.Tasks;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public interface IBackupService
    {
        Task<IReadOnlyList<BackupRecord>> GetBackupsAsync();
        Task<BackupRecord> CreateBackupAsync(ServerInstance server);
    }
}
