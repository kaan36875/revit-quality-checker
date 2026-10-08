using System.Collections.Generic;
using RevitQualityChecker.Core.Models;
using RevitQualityChecker.Core.Rules;
using Xunit;

namespace RevitQualityChecker.Core.Tests.Rules
{
    public class DuplicateNameRuleTests
    {
        private readonly DuplicateNameRule _rule = new DuplicateNameRule(
            "HLT-001", "Duplicate View Names", "Detects duplicate view names",
            Severity.Warning, "Views");

        [Fact]
        public void PassesWhenAllNamesUnique()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo { Id = "1", Name = "Floor Plan - L1", Category = "Views" },
                new ElementInfo { Id = "2", Name = "Floor Plan - L2", Category = "Views" },
                new ElementInfo { Id = "3", Name = "Section 1", Category = "Views" }
            };

            var result = _rule.Evaluate(elements);

            Assert.True(result.Passed);
            Assert.Equal(3, result.CheckedCount);
        }

        [Fact]
        public void DetectsDuplicateNames()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo { Id = "1", Name = "L5", Category = "Views" },
                new ElementInfo { Id = "2", Name = "L5", Category = "Views" },
                new ElementInfo { Id = "3", Name = "Section 1", Category = "Views" }
            };

            var result = _rule.Evaluate(elements);

            Assert.False(result.Passed);
            Assert.Equal(3, result.CheckedCount);
            Assert.Equal(2, result.Violations.Count);
            Assert.Contains("shared by 2 elements", result.Violations[0].Message);
        }

        [Fact]
        public void SkipsOtherCategories()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo { Id = "1", Name = "Wall-A", Category = "Walls" },
                new ElementInfo { Id = "2", Name = "Wall-A", Category = "Walls" }
            };

            var result = _rule.Evaluate(elements);

            Assert.True(result.Passed);
            Assert.Equal(0, result.CheckedCount);
        }

        [Fact]
        public void HandlesEmptyList()
        {
            var elements = new List<ElementInfo>();

            var result = _rule.Evaluate(elements);

            Assert.True(result.Passed);
            Assert.Equal(0, result.CheckedCount);
        }
    }
}
