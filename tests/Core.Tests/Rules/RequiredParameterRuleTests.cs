using System.Collections.Generic;
using RevitQualityChecker.Core.Models;
using RevitQualityChecker.Core.Rules;
using Xunit;

namespace RevitQualityChecker.Core.Tests.Rules
{
    public class RequiredParameterRuleTests
    {
        private readonly RequiredParameterRule _rule = new RequiredParameterRule(
            "PAR-001", "Fire Rating Required", "Walls need fire rating",
            Severity.Error, "Walls", "Fire Rating");

        [Fact]
        public void PassesWhenParameterExists()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo
                {
                    Id = "1", Name = "Wall1", Category = "Walls",
                    Parameters = new Dictionary<string, string> { { "Fire Rating", "60" } }
                }
            };

            var result = _rule.Evaluate(elements);

            Assert.True(result.Passed);
            Assert.Equal(1, result.CheckedCount);
        }

        [Fact]
        public void FailsWhenParameterMissing()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo
                {
                    Id = "1", Name = "Wall1", Category = "Walls",
                    Parameters = new Dictionary<string, string>()
                }
            };

            var result = _rule.Evaluate(elements);

            Assert.False(result.Passed);
            Assert.Single(result.Violations);
            Assert.Contains("Fire Rating", result.Violations[0].Message);
        }

        [Fact]
        public void FailsWhenParameterIsWhitespace()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo
                {
                    Id = "1", Name = "Wall1", Category = "Walls",
                    Parameters = new Dictionary<string, string> { { "Fire Rating", "  " } }
                }
            };

            var result = _rule.Evaluate(elements);

            Assert.False(result.Passed);
        }

        [Fact]
        public void SkipsOtherCategories()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo
                {
                    Id = "1", Name = "Door1", Category = "Doors",
                    Parameters = new Dictionary<string, string>()
                }
            };

            var result = _rule.Evaluate(elements);

            Assert.True(result.Passed);
            Assert.Equal(0, result.CheckedCount);
        }
    }
}
