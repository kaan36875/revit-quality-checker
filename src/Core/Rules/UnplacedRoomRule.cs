using System.Collections.Generic;
using RevitQualityChecker.Core.Models;

namespace RevitQualityChecker.Core.Rules
{
    public class UnplacedRoomRule : IRule
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public Severity Severity { get; }
        public string Category => "Model Health";

        public UnplacedRoomRule(string id, string name, string description, Severity severity)
        {
            Id = id;
            Name = name;
            Description = description;
            Severity = severity;
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
                if (element.Category != "Rooms")
                    continue;

                result.CheckedCount++;

                bool hasArea = element.Parameters.TryGetValue("Area", out var areaStr)
                    && !string.IsNullOrWhiteSpace(areaStr)
                    && double.TryParse(areaStr, out var area)
                    && area > 0;

                if (!hasArea)
                {
                    result.Violations.Add(new RuleViolation
                    {
                        ElementId = element.Id,
                        ElementName = element.Name ?? "(empty)",
                        Category = element.Category,
                        Message = "Room appears unplaced (area is zero or missing)",
                        SuggestedFix = "Place the room within bounded walls or delete if unused"
                    });
                }
            }

            return result;
        }
    }
}
