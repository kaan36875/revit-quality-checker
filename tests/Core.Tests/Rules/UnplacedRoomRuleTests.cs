using System.Collections.Generic;
using RevitQualityChecker.Core.Models;
using RevitQualityChecker.Core.Rules;
using Xunit;

namespace RevitQualityChecker.Core.Tests.Rules
{
    public class UnplacedRoomRuleTests
    {
        private readonly UnplacedRoomRule _rule = new UnplacedRoomRule(
            "HLT-002", "Unplaced Rooms", "Detects unplaced rooms",
            Severity.Error);

        [Fact]
        public void PassesWhenRoomHasArea()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo
                {
                    Id = "1", Name = "Room 101", Category = "Rooms",
                    Parameters = new Dictionary<string, string> { { "Area", "150" } }
                }
            };

            var result = _rule.Evaluate(elements);

            Assert.True(result.Passed);
            Assert.Equal(1, result.CheckedCount);
        }

        [Fact]
        public void FailsWhenAreaIsZero()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo
                {
                    Id = "1", Name = "Room 102", Category = "Rooms",
                    Parameters = new Dictionary<string, string> { { "Area", "0" } }
                }
            };

            var result = _rule.Evaluate(elements);

            Assert.False(result.Passed);
            Assert.Single(result.Violations);
        }

        [Fact]
        public void FailsWhenAreaParameterMissing()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo { Id = "1", Name = "Room 103", Category = "Rooms" }
            };

            var result = _rule.Evaluate(elements);

            Assert.False(result.Passed);
            Assert.Single(result.Violations);
        }

        [Fact]
        public void SkipsNonRooms()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo { Id = "1", Name = "Wall-A", Category = "Walls" }
            };

            var result = _rule.Evaluate(elements);

            Assert.True(result.Passed);
            Assert.Equal(0, result.CheckedCount);
        }

        [Fact]
        public void FailsWhenAreaIsNonNumeric()
        {
            var elements = new List<ElementInfo>
            {
                new ElementInfo
                {
                    Id = "1", Name = "Room 104", Category = "Rooms",
                    Parameters = new Dictionary<string, string> { { "Area", "N/A" } }
                }
            };

            var result = _rule.Evaluate(elements);

            Assert.False(result.Passed);
            Assert.Single(result.Violations);
        }
    }
}
