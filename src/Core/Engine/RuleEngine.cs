using System;
using System.Collections.Generic;
using RevitQualityChecker.Core.Models;
using RevitQualityChecker.Core.Rules;

namespace RevitQualityChecker.Core.Engine
{
    public class RuleEngine
    {
        private readonly List<IRule> _rules = new List<IRule>();

        public IReadOnlyList<IRule> Rules => _rules;

        public void AddRule(IRule rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            _rules.Add(rule);
        }

        public void AddRules(IEnumerable<IRule> rules)
        {
            foreach (var rule in rules)
                AddRule(rule);
        }

        public CheckReport Run(string modelName, IReadOnlyList<ElementInfo> elements)
        {
            var report = new CheckReport
            {
                ModelName = modelName,
                Timestamp = DateTime.UtcNow
            };

            foreach (var rule in _rules)
            {
                var result = rule.Evaluate(elements);
                report.Results.Add(result);
            }

            return report;
        }
    }
}
