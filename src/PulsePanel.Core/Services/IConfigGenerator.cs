using PulsePanel.Core.Models;
using System.Threading.Tasks;

namespace PulsePanel.Core.Services
{
    public interface IConfigGenerator
    {
        Task<GenerationResult> GenerateAsync(string blueprintPath, string valuesPath, string outputRoot);
    }
}
