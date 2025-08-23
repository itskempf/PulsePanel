using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public sealed class ComplianceService : IComplianceService
    {
        public Task<ComplianceReport> ScanAsync(Blueprint bp, string? nodeId, CancellationToken ct)
        {
            // Minimal sample: interpret some rules baked into blueprint metadata
            var rules = bp.ComplianceRules ?? new List<ComplianceRule>();
            var results = new List<ComplianceResult>();

            foreach (var r in rules)
            {
                ct.ThrowIfCancellationRequested();
                results.Add(CheckRule(r));
            }

            var overall = results.Any(x => x.Status == ComplianceStatus.Fail) ? ComplianceStatus.Fail
                         : results.Any(x => x.Status == ComplianceStatus.Warn) ? ComplianceStatus.Warn
                         : ComplianceStatus.Pass;

            return Task.FromResult(new ComplianceReport
            {
                BlueprintName = bp.Name,
                NodeId = nodeId,
                Results = results,
                Overall = overall
            });
        }

        private static ComplianceResult CheckRule(ComplianceRule r)
        {
            try
            {
                return r.CheckType switch
                {
                    "FileExists" => FileExists(r),
                    _ => new ComplianceResult { Rule = r, Status = ComplianceStatus.Warn, Message = "Unknown check type." }
                };
            }
            catch (Exception ex)
            {
                return new ComplianceResult { Rule = r, Status = ComplianceStatus.Fail, Message = ex.Message };
            }
        }

        private static ComplianceResult FileExists(ComplianceRule r)
        {
            var ok = File.Exists(r.Target) || Directory.Exists(r.Target);
            return new ComplianceResult
            {
                Rule = r,
                Status = ok ? ComplianceStatus.Pass : ComplianceStatus.Fail,
                Message = ok ? "Found." : "Missing."
            };
        }
    }
}