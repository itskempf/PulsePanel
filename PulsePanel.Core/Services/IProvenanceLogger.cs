
using System.Threading.Tasks;

namespace PulsePanel.Core.Services
{
    public interface IProvenanceLogger
    {
        Task LogAsync(string action, string detail);
    }
}
