using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitQualityChecker.Core.Models
{
    public class CheckReport
    {
        public string ModelName { get; set; }
        public DateTime Timestamp { get; set; }
        public List<RuleResult> Results { get; set; } = new List<RuleResult>();

        public int TotalChecked => Results.Sum(r => r.CheckedCount);
        public int TotalViolations => Results.Sum(r => r.Violations.Count);
        public int ErrorCount => Results.Where(r => r.Severity == Severity.Error).Sum(r => r.Violations.Count);
        public int WarningCount => Results.Where(r => r.Severity == Severity.Warning).Sum(r => r.Violations.Count);
        public int InfoCount => Results.Where(r => r.Severity == Severity.Info).Sum(r => r.Violations.Count);

        public double Score
        {
            get
            {
                if (TotalChecked == 0) return 100.0;
                int passed = TotalChecked - TotalViolations;
                double ratio = (double)Math.Max(0, passed) / TotalChecked;
                return Math.Round(ratio * 100, 1);
            }
        }
    }
}
