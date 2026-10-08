using System.Collections.Generic;
using RevitQualityChecker.Core.Models;
using RevitQualityChecker.Core.Rules;
using Xunit;

namespace RevitQualityChecker.Core.Tests.Rules
{
    public class DuplicateParameterRuleTests
    {
        private DuplicateParameterRule CreateRule(string targetCategory = "Rooms", string parameterName = "Number")
        {
            return new DuplicateParameterRule("HLT-003", "Duplicate Room Numbers", "Test",
                Severity.Error, targetCategory, parameterName);
        }

        [Fact]
        public void UniqueValues_NoDuplicates()
        {
            var rule = CreateRule();
            var elements = new List<ElementInfo>
            {
                new() { Id = "1", Name = "Room A", Category = "Rooms", Parameters = new() { ["Number"] = "101" } },
                new() { Id = "2", Name = "Room B", Category = "Rooms", Parameters = new() { ["Number"] = "102" } },
                new() { Id = "3", Name = "Room C", Category = "Rooms", Parameters = new() { ["Number"] = "103" } },
            };

            var result = rule.Evaluate(elements);

            Assert.True(result.Passed);
            Assert.Equal(3, result.CheckedCount);
        }

        [Fact]
        public void DuplicateValues_FlagsAll()
        {
            var rule = CreateRule();
            var elements = new List<ElementInfo>
            {
                new() { Id = "1", Name = "Room A", Category = "Rooms", Parameters = new() { ["Number"] = "101" } },
                new() { Id = "2", Name = "Room B", Category = "Rooms", Parameters = new() { ["Number"] = "101" } },
                new() { Id = "3", Name = "Room C", Category = "Rooms", Parameters = new() { ["Number"] = "102" } },
            };

            var result = rule.Evaluate(elements);

            Assert.False(result.Passed);
            Assert.Equal(2, result.Violations.Count);
        }

        [Fact]
        public void EmptyValues_NotDuplicate()
        {
            var rule = CreateRule();
            var elements = new List<ElementInfo>
            {
                new() { Id = "1", Name = "Room A", Category = "Rooms", Parameters = new() { ["Number"] = "" } },
                new() { Id = "2", Name = "Room B", Category = "Rooms", Parameters = new() { ["Number"] = "" } },
            };

            var result = rule.Evaluate(elements);

            Assert.True(result.Passed);
        }

        [Fact]
        public void FiltersCategory()
        {
            var rule = CreateRule("Rooms");
            var elements = new List<ElementInfo>
            {
                new() { Id = "1", Name = "Room A", Category = "Rooms", Parameters = new() { ["Number"] = "101" } },
                new() { Id = "2", Name = "Door A", Category = "Doors", Parameters = new() { ["Number"] = "101" } },
            };

            var result = rule.Evaluate(elements);

            Assert.True(result.Passed);
            Assert.Equal(1, result.CheckedCount);
        }

        [Fact]
        public void MissingParameter_Ignored()
        {
            var rule = CreateRule();
            var elements = new List<ElementInfo>
            {
                new() { Id = "1", Name = "Room A", Category = "Rooms", Parameters = new() },
                new() { Id = "2", Name = "Room B", Category = "Rooms", Parameters = new() },
            };

            var result = rule.Evaluate(elements);

            Assert.True(result.Passed);
            Assert.Equal(2, result.CheckedCount);
        }
    }
}
