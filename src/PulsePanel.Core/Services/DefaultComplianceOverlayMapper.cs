using System.Collections.Generic;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services;

public class DefaultComplianceOverlayMapper : IComplianceOverlayMapper
{
    public IEnumerable<ComplianceOverlayItem> Map(PnccLValidationResult validationResult)
    {
        var items = new List<ComplianceOverlayItem>();
        foreach (var f in validationResult.Findings)
        {
            items.Add(new ComplianceOverlayItem
            {
                LineNumber = 0, // Unknown without parser context; UI can decorate globally
                ColumnStart = 0,
                ColumnEnd = 0,
                RuleId = f.RuleId ?? f.Code,
                Severity = f.Severity,
                Message = f.Message
            });
        }
        return items;
    }
}
