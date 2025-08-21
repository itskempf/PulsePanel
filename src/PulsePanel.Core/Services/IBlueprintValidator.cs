using PulsePanel.Blueprints;
using PulsePanel.Blueprints.Models;
using System.Threading.Tasks;

namespace PulsePanel.Core.Services
{
    public interface IBlueprintValidator
    {
        Task<ValidationResult> ValidateBlueprintAsync(string blueprintPath);
    }
}
