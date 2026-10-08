using System.Collections.Generic;
using System.Linq;
using RevitQualityChecker.Core.Models;

namespace RevitQualityChecker.Core.Rules
{
    public class DuplicateParameterRule : IRule
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public Severity Severity { get; }
        public string Category => "Model Health";

        private readonly string _targetCategory;
        private readonly string _parameterName;

        public DuplicateParameterRule(string id, string name, string description, Severity severity,
            string targetCategory, string parameterName)
        {
            Id = id;
            Name = name;
            Description = description;
            Severity = severity;
            _targetCategory = targetCategory;
            _parameterName = parameterName;
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

            var filtered = elements
                .Where(e => string.IsNullOrEmpty(_targetCategory) || e.Category == _targetCategory)
                .ToList();

            result.CheckedCount = filtered.Count;

            var groups = filtered
                .Where(e => e.Parameters.ContainsKey(_parameterName)
                             && !string.IsNullOrWhiteSpace(e.Parameters[_parameterName]))
                .GroupBy(e => e.Parameters[_parameterName])
                .Where(g => g.Count() > 1);

            foreach (var group in groups)
            {
                foreach (var el in group)
                {
                    result.Violations.Add(new RuleViolation
                    {
                        ElementId = el.Id,
                        ElementName = el.Name,
                        Category = el.Category,
                        Message = $"Duplicate {_parameterName}: '{group.Key}' (shared by {group.Count()} elements)",
                        SuggestedFix = $"Assign a unique {_parameterName}"
                    });
                }
            }

            return result;
        }
    }
}
