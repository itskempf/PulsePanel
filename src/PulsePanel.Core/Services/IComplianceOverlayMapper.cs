using PulsePanel.Core.Models;
using System.Collections.Generic;

namespace PulsePanel.Core.Services
{
    public interface IComplianceOverlayMapper
    {
        IEnumerable<ComplianceOverlayItem> Map(PnccLValidationResult validationResult);
    }
}
