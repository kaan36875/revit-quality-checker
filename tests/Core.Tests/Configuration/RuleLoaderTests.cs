using System;
using RevitQualityChecker.Core.Configuration;
using Xunit;

namespace RevitQualityChecker.Core.Tests.Configuration
{
    public class RuleLoaderTests
    {
        private readonly RuleLoader _loader = new RuleLoader();

        private const string ValidJson = @"{
            ""name"": ""Test Rules"",
            ""version"": ""1.0.0"",
            ""rules"": [
                {
                    ""id"": ""NAM-001"",
                    ""name"": ""Wall Naming"",
                    ""description"": ""Walls must follow naming pattern"",
                    ""type"": ""naming"",
                    ""severity"": ""Error"",
                    ""targetCategory"": ""Walls"",
                    ""parameters"": { ""pattern"": ""^[A-Za-z]+_\\d+$"" }
                },
                {
                    ""id"": ""PAR-001"",
                    ""name"": ""Fire Rating"",
                    ""description"": ""Fire rating required"",
                    ""type"": ""required_parameter"",
                    ""severity"": ""Warning"",
                    ""targetCategory"": ""Walls"",
                    ""parameters"": { ""parameterName"": ""Fire Rating"" }
                }
            ]
        }";

        [Fact]
        public void LoadsValidJson()
        {
            var definition = _loader.LoadFromJson(ValidJson);

            Assert.Equal("Test Rules", definition.Name);
            Assert.Equal(2, definition.Rules.Count);
        }

        [Fact]
        public void CreatesRulesFromDefinition()
        {
            var definition = _loader.LoadFromJson(ValidJson);
            var rules = _loader.CreateRules(definition);

            Assert.Equal(2, rules.Count);
            Assert.Equal("NAM-001", rules[0].Id);
            Assert.Equal("PAR-001", rules[1].Id);
        }

        [Fact]
        public void ThrowsOnMissingName()
        {
            var json = @"{ ""rules"": [{ ""id"": ""T-001"", ""type"": ""naming"" }] }";
            Assert.Throws<InvalidOperationException>(() => _loader.LoadFromJson(json));
        }

        [Fact]
        public void ThrowsOnDuplicateIds()
        {
            var json = @"{
                ""name"": ""Test"",
                ""rules"": [
                    { ""id"": ""T-001"", ""name"": ""R1"", ""type"": ""naming"", ""parameters"": { ""pattern"": "".*"" } },
                    { ""id"": ""T-001"", ""name"": ""R2"", ""type"": ""naming"", ""parameters"": { ""pattern"": "".*"" } }
                ]
            }";
            Assert.Throws<InvalidOperationException>(() => _loader.LoadFromJson(json));
        }

        [Fact]
        public void ThrowsOnUnknownRuleType()
        {
            var json = @"{
                ""name"": ""Test"",
                ""rules"": [
                    { ""id"": ""T-001"", ""name"": ""R1"", ""type"": ""unknown_type"" }
                ]
            }";
            var def = _loader.LoadFromJson(json);
            Assert.Throws<InvalidOperationException>(() => _loader.CreateRules(def));
        }

        [Fact]
        public void ThrowsOnMissingRequiredParameter()
        {
            var json = @"{
                ""name"": ""Test"",
                ""rules"": [
                    { ""id"": ""T-001"", ""name"": ""R1"", ""type"": ""naming"", ""parameters"": {} }
                ]
            }";
            var def = _loader.LoadFromJson(json);
            Assert.Throws<InvalidOperationException>(() => _loader.CreateRules(def));
        }

        [Fact]
        public void CreatesDuplicateNameRule()
        {
            var json = @"{
                ""name"": ""Test"",
                ""rules"": [
                    { ""id"": ""HLT-001"", ""name"": ""Dup Views"", ""type"": ""duplicate_name"", ""targetCategory"": ""Views"" }
                ]
            }";
            var def = _loader.LoadFromJson(json);
            var rules = _loader.CreateRules(def);

            Assert.Single(rules);
            Assert.Equal("HLT-001", rules[0].Id);
            Assert.IsType<RevitQualityChecker.Core.Rules.DuplicateNameRule>(rules[0]);
        }

        [Fact]
        public void CreatesDuplicateParameterRule()
        {
            var json = @"{
                ""name"": ""Test"",
                ""rules"": [
                    { ""id"": ""HLT-003"", ""name"": ""Dup Numbers"", ""type"": ""duplicate_parameter"", ""targetCategory"": ""Rooms"", ""parameters"": { ""parameterName"": ""Number"" } }
                ]
            }";
            var def = _loader.LoadFromJson(json);
            var rules = _loader.CreateRules(def);

            Assert.Single(rules);
            Assert.Equal("HLT-003", rules[0].Id);
            Assert.IsType<RevitQualityChecker.Core.Rules.DuplicateParameterRule>(rules[0]);
        }

        [Fact]
        public void CreatesUnplacedRoomRule()
        {
            var json = @"{
                ""name"": ""Test"",
                ""rules"": [
                    { ""id"": ""HLT-002"", ""name"": ""Unplaced"", ""type"": ""unplaced_room"" }
                ]
            }";
            var def = _loader.LoadFromJson(json);
            var rules = _loader.CreateRules(def);

            Assert.Single(rules);
            Assert.Equal("HLT-002", rules[0].Id);
            Assert.IsType<RevitQualityChecker.Core.Rules.UnplacedRoomRule>(rules[0]);
        }
    }
}
