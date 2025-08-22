using System;
using System.Collections.Generic;

namespace PulsePanel.Core.Models
{
    public class ConfigProfile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> Settings { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ProvenanceId { get; set; }
    }

    public class ConfigDiff
    {
        public List<DiffLine> Lines { get; set; }
    }

    public class DiffLine
    {
        public DiffLineType Type { get; set; }
        public string Text { get; set; }
    }

    public enum DiffLineType
    {
        Unchanged,
        Added,
        Removed
    }

    public class ComplianceOverlayItem
    {
        public int LineNumber { get; set; }
        public int ColumnStart { get; set; }
        public int ColumnEnd { get; set; }
        public string RuleId { get; set; }
        public ValidationSeverity Severity { get; set; }
        public string Message { get; set; }
    }

    public class CommitResult
    {
        public bool Success { get; set; }
        public string CommitId { get; set; }
        public string ProvenanceId { get; set; }
    }
}
