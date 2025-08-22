using PulsePanel.Core.Models;
using System.Threading.Tasks;

namespace PulsePanel.Core.Services
{
    public interface IConfigValidationProvider
    {
        Task<PnccLValidationResult> ValidateAsync(string content, string fileType);
    }
}
