using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RevitQualityChecker.Core.Models;
using RevitQualityChecker.Core.Rules;

namespace RevitQualityChecker.Core.Configuration
{
    public class RuleLoader
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() }
        };

        public RuleSetDefinition LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Rule file not found: {filePath}");

            var json = File.ReadAllText(filePath);
            return LoadFromJson(json);
        }

        public RuleSetDefinition LoadFromJson(string json)
        {
            var definition = JsonConvert.DeserializeObject<RuleSetDefinition>(json, Settings);
            if (definition == null)
                throw new InvalidOperationException("Failed to parse rule file.");

            Validate(definition);
            return definition;
        }

        public IReadOnlyList<IRule> CreateRules(RuleSetDefinition definition)
        {
            var rules = new List<IRule>();

            foreach (var ruleDef in definition.Rules)
            {
                var rule = CreateRule(ruleDef);
                if (rule != null)
                    rules.Add(rule);
            }

            return rules;
        }

        private IRule CreateRule(RuleDefinition def)
        {
            switch (def.Type.ToLowerInvariant())
            {
                case "naming":
                    if (!def.Parameters.TryGetValue("pattern", out var pattern))
                        throw new InvalidOperationException($"Rule '{def.Id}': naming rule requires a 'pattern' parameter.");
                    def.Parameters.TryGetValue("violationMessage", out var namViolMsg);
                    def.Parameters.TryGetValue("suggestedFix", out var namSugFix);
                    return new NamingRule(def.Id, def.Name, def.Description, def.Severity,
                        def.TargetCategory, pattern, namViolMsg, namSugFix);

                case "required_parameter":
                    if (!def.Parameters.TryGetValue("parameterName", out var paramName))
                        throw new InvalidOperationException($"Rule '{def.Id}': required_parameter rule requires a 'parameterName' parameter.");
                    def.Parameters.TryGetValue("violationMessage", out var violMsg);
                    def.Parameters.TryGetValue("suggestedFix", out var sugFix);
                    return new RequiredParameterRule(def.Id, def.Name, def.Description, def.Severity,
                        def.TargetCategory, paramName, violMsg, sugFix);

                case "duplicate_name":
                    return new DuplicateNameRule(def.Id, def.Name, def.Description, def.Severity,
                        def.TargetCategory);

                case "duplicate_parameter":
                    if (!def.Parameters.TryGetValue("parameterName", out var dupParamName))
                        throw new InvalidOperationException($"Rule '{def.Id}': duplicate_parameter rule requires a 'parameterName' parameter.");
                    return new DuplicateParameterRule(def.Id, def.Name, def.Description, def.Severity,
                        def.TargetCategory, dupParamName);

                case "unplaced_room":
                    return new UnplacedRoomRule(def.Id, def.Name, def.Description, def.Severity);

                default:
                    throw new InvalidOperationException($"Rule '{def.Id}': unknown rule type '{def.Type}'.");
            }
        }

        private void Validate(RuleSetDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(definition.Name))
                throw new InvalidOperationException("Rule set must have a name.");

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var rule in definition.Rules)
            {
                if (string.IsNullOrWhiteSpace(rule.Id))
                    throw new InvalidOperationException("Every rule must have an id.");
                if (string.IsNullOrWhiteSpace(rule.Type))
                    throw new InvalidOperationException($"Rule '{rule.Id}' must have a type.");
                if (!ids.Add(rule.Id))
                    throw new InvalidOperationException($"Duplicate rule id: '{rule.Id}'.");
            }
        }
    }
}
