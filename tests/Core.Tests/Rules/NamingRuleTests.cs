using System.Collections.Generic;
using RevitQualityChecker.Core.Models;
using RevitQualityChecker.Core.Rules;
using Xunit;

namespace RevitQualityChecker.Core.Tests.Rules
{
    public class NamingRuleTests
    {
        private readonly NamingRule _rule = new NamingRule(
            "NAM-001", "Wall Naming", "Walls must match pattern",
            Severity.Error, "Walls", @"^[A-Za-z]+_\d+$");

        [Fact]
        public void PassesWhenNameMatchesPattern()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo { Id = "1", Name = "Brick_200", Category = "Walls" }
            };

            var result = _rule.Evaluate(elements);

            Assert.True(result.Passed);
            Assert.Equal(1, result.CheckedCount);
        }

        [Fact]
        public void FailsWhenNameDoesNotMatchPattern()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo { Id = "1", Name = "My Wall Type", Category = "Walls" }
            };

            var result = _rule.Evaluate(elements);

            Assert.False(result.Passed);
            Assert.Single(result.Violations);
            Assert.Contains("My Wall Type", result.Violations[0].Message);
        }

        [Fact]
        public void SkipsElementsFromOtherCategories()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo { Id = "1", Name = "Any Name", Category = "Doors" }
            };

            var result = _rule.Evaluate(elements);

            Assert.True(result.Passed);
            Assert.Equal(0, result.CheckedCount);
        }

        [Fact]
        public void FailsWhenNameIsEmpty()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo { Id = "1", Name = "", Category = "Walls" }
            };

            var result = _rule.Evaluate(elements);

            Assert.False(result.Passed);
            Assert.Single(result.Violations);
        }

        [Fact]
        public void ChecksMultipleElements()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo { Id = "1", Name = "Brick_200", Category = "Walls" },
                new ElementInfo { Id = "2", Name = "Bad Name", Category = "Walls" },
                new ElementInfo { Id = "3", Name = "Concrete_300", Category = "Walls" }
            };

            var result = _rule.Evaluate(elements);

            Assert.Equal(3, result.CheckedCount);
            Assert.Single(result.Violations);
            Assert.Equal("2", result.Violations[0].ElementId);
        }
    }
}
