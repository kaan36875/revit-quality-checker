using System.Collections.Generic;
using RevitQualityChecker.Core.Models;

namespace RevitQualityChecker.Core.Configuration
{
    public class RuleSetDefinition
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public List<RuleDefinition> Rules { get; set; } = new List<RuleDefinition>();
    }

    public class RuleDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public Severity Severity { get; set; } = Severity.Warning;
        public string TargetCategory { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}
