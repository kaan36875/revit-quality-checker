using System.Collections.Generic;
using System.Text.RegularExpressions;
using RevitQualityChecker.Core.Models;

namespace RevitQualityChecker.Core.Rules
{
    public class NamingRule : IRule
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public Severity Severity { get; }
        public string Category => "Naming";

        private readonly string _targetCategory;
        private readonly string _pattern;
        private readonly Regex _regex;
        private readonly string _violationMessage;
        private readonly string _suggestedFix;

        public NamingRule(string id, string name, string description, Severity severity,
            string targetCategory, string pattern,
            string violationMessage = null, string suggestedFix = null)
        {
            Id = id;
            Name = name;
            Description = description;
            Severity = severity;
            _targetCategory = targetCategory;
            _pattern = pattern;
            _regex = new Regex(pattern);
            _violationMessage = violationMessage;
            _suggestedFix = suggestedFix;
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

            foreach (var element in elements)
            {
                if (!string.IsNullOrEmpty(_targetCategory) && element.Category != _targetCategory)
                    continue;

                result.CheckedCount++;

                if (string.IsNullOrEmpty(element.Name) || !_regex.IsMatch(element.Name))
                {
                    result.Violations.Add(new RuleViolation
                    {
                        ElementId = element.Id,
                        ElementName = element.Name ?? "(empty)",
                        Category = element.Category,
                        Message = _violationMessage != null
                            ? string.Format(_violationMessage, element.Name)
                            : $"Name '{element.Name}' does not follow naming convention",
                        SuggestedFix = _suggestedFix ?? "Rename with a meaningful, descriptive name"
                    });
                }
            }

            return result;
        }
    }
}
