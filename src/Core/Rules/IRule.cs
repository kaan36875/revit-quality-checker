using System.Collections.Generic;
using RevitQualityChecker.Core.Models;

namespace RevitQualityChecker.Core.Rules
{
    public interface IRule
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        Severity Severity { get; }
        string Category { get; }
        RuleResult Evaluate(IReadOnlyList<ElementInfo> elements);
    }
}
