using System.Collections.Generic;
using RevitQualityChecker.Core.Models;

namespace RevitQualityChecker.Core.Rules
{
    public class RequiredParameterRule : IRule
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public Severity Severity { get; }
        public string Category => "Parameters";

        private readonly string _targetCategory;
        private readonly string _parameterName;
        private readonly string _violationMessage;
        private readonly string _suggestedFix;

        public RequiredParameterRule(string id, string name, string description, Severity severity,
            string targetCategory, string parameterName,
            string violationMessage = null, string suggestedFix = null)
        {
            Id = id;
            Name = name;
            Description = description;
            Severity = severity;
            _targetCategory = targetCategory;
            _parameterName = parameterName;
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

                bool hasParam = element.Parameters.TryGetValue(_parameterName, out var value)
                    && !string.IsNullOrWhiteSpace(value);

                if (!hasParam)
                {
                    result.Violations.Add(new RuleViolation
                    {
                        ElementId = element.Id,
                        ElementName = element.Name,
                        Category = element.Category,
                        Message = _violationMessage ?? $"Required parameter '{_parameterName}' is missing or empty",
                        SuggestedFix = _suggestedFix ?? $"Fill in the '{_parameterName}' parameter"
                    });
                }
            }

            return result;
        }
    }
}
