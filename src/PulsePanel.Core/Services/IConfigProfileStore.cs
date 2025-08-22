using PulsePanel.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PulsePanel.Core.Services
{
    public interface IConfigProfileStore
    {
        Task<IEnumerable<string>> GetProfileNamesAsync(string configFilePath);
        Task<ConfigProfile> LoadProfileAsync(string configFilePath, string profileName);
        Task SaveProfileAsync(string configFilePath, ConfigProfile profile);
        Task DeleteProfileAsync(string configFilePath, string profileName);
    }
}
