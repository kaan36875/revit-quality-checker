using System.Collections.Generic;
using System.Linq;
using RevitQualityChecker.Core.Models;

namespace RevitQualityChecker.Core.Rules
{
    public class DuplicateNameRule : IRule
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public Severity Severity { get; }
        public string Category => "Model Health";

        private readonly string _targetCategory;

        public DuplicateNameRule(string id, string name, string description, Severity severity,
            string targetCategory)
        {
            Id = id;
            Name = name;
            Description = description;
            Severity = severity;
            _targetCategory = targetCategory;
        }

        public RuleResult Evaluate(IReadOnlyList<ElementInfo> elements)
        {
            var result = new RuleResult
            {
                RuleId = Id,
                RuleName = Name,
                RuleCategory = Category,
                Severity = Severity
            };

            var filtered = new List<ElementInfo>();
            foreach (var element in elements)
            {
                if (!string.IsNullOrEmpty(_targetCategory) && element.Category != _targetCategory)
                    continue;
                filtered.Add(element);
            }

            result.CheckedCount = filtered.Count;

            var groups = filtered.GroupBy(e => e.Name ?? "").Where(g => g.Count() > 1);

            foreach (var group in groups)
            {
                int count = group.Count();
                foreach (var element in group)
                {
                    result.Violations.Add(new RuleViolation
                    {
                        ElementId = element.Id,
                        ElementName = element.Name ?? "(empty)",
                        Category = element.Category,
                        Message = $"Duplicate name '{element.Name}' shared by {count} elements",
                        SuggestedFix = "Rename to make unique"
                    });
                }
            }

            return result;
        }
    }
}
