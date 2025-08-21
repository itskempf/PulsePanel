using System.Collections.Generic;

namespace PulsePanel.Core.Models
{
    public class GenerationResult
    {
        public bool Success { get; set; }
        public string? OutputPath { get; set; }
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, string> GeneratedFileHashes { get; set; } = new();
    }
}
