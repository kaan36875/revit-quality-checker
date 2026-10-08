using System;
using System.Collections.Generic;
using RevitQualityChecker.Core.Engine;
using RevitQualityChecker.Core.Models;
using RevitQualityChecker.Core.Rules;
using Xunit;

namespace RevitQualityChecker.Core.Tests.Engine
{
    public class RuleEngineTests
    {
        [Fact]
        public void RunsAllRulesAndReturnsReport()
        {
            var engine = new RuleEngine();
            engine.AddRule(new NamingRule("NAM-001", "Wall Naming", "test",
                Severity.Error, "Walls", @"^[A-Za-z]+_\d+$"));
            engine.AddRule(new RequiredParameterRule("PAR-001", "Fire Rating", "test",
                Severity.Warning, "Walls", "Fire Rating"));

            var elements = new List<ElementInfo>
            {
                new ElementInfo
                {
                    Id = "1", Name = "Bad Name", Category = "Walls",
                    Parameters = new Dictionary<string, string> { { "Fire Rating", "60" } }
                },
                new ElementInfo
                {
                    Id = "2", Name = "Brick_200", Category = "Walls",
                    Parameters = new Dictionary<string, string>()
                }
            };

            var report = engine.Run("TestModel.rvt", elements);

            Assert.Equal("TestModel.rvt", report.ModelName);
            Assert.Equal(2, report.Results.Count);
            Assert.Equal(1, report.ErrorCount);
            Assert.Equal(1, report.WarningCount);
        }

        [Fact]
        public void ScoreIs100WhenNoViolations()
        {
            var engine = new RuleEngine();
            engine.AddRule(new NamingRule("NAM-001", "Wall Naming", "test",
                Severity.Error, "Walls", @"^[A-Za-z]+_\d+$"));

            var elements = new List<ElementInfo>
            {
                new ElementInfo { Id = "1", Name = "Brick_200", Category = "Walls" }
            };

            var report = engine.Run("Clean.rvt", elements);

            Assert.Equal(100.0, report.Score);
        }

        [Fact]
        public void ScoreDecreasesWithViolations()
        {
            var engine = new RuleEngine();
            engine.AddRule(new NamingRule("NAM-001", "Wall Naming", "test",
                Severity.Error, "Walls", @"^[A-Za-z]+_\d+$"));

            var elements = new List<ElementInfo>
            {
                new ElementInfo { Id = "1", Name = "Bad", Category = "Walls" },
                new ElementInfo { Id = "2", Name = "Brick_200", Category = "Walls" }
            };

            var report = engine.Run("Mixed.rvt", elements);

            Assert.True(report.Score < 100.0);
            Assert.True(report.Score > 0);
        }

        [Fact]
        public void ReportTimestampIsSet()
        {
            var engine = new RuleEngine();
            var before = DateTime.UtcNow;
            var report = engine.Run("Test.rvt", new List<ElementInfo>());
            var after = DateTime.UtcNow;

            Assert.InRange(report.Timestamp, before, after);
        }
    }
}
