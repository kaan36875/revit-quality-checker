using System.Collections.Generic;

namespace RevitQualityChecker.Core.Models
{
    public class RuleResult
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string RuleCategory { get; set; }
        public Severity Severity { get; set; }
        public List<RuleViolation> Violations { get; set; } = new List<RuleViolation>();

        public bool Passed => Violations.Count == 0;
        public int CheckedCount { get; set; }
    }

    public class RuleViolation
    {
        public string ElementId { get; set; }
        public string ElementName { get; set; }
        public string Category { get; set; }
        public string Message { get; set; }
        public string SuggestedFix { get; set; }
    }
}
