using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitQualityChecker.Core.Models;
using RevitQualityChecker.RevitAdapter.Compat;

namespace RevitQualityChecker.RevitAdapter
{
    public class AutoFixResult
    {
        public int FixedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Messages { get; set; } = new();
    }

    public class AutoFixer
    {
        private static readonly Dictionary<string, Func<Document, RuleResult, AutoFixResult>> Fixers = new()
        {
            ["HLT-006"] = FixUnjoinedWalls,
        };

        public static bool CanFix(string ruleId) => Fixers.ContainsKey(ruleId);

        public static AutoFixResult Fix(Document doc, RuleResult result)
        {
            if (!Fixers.TryGetValue(result.RuleId, out var fixer))
                return new AutoFixResult { Messages = { $"No auto-fix available for {result.RuleId}" } };

            using var tx = new Transaction(doc, $"Quality Checker: Fix {result.RuleName}");
            tx.Start();
            try
            {
                var fixResult = fixer(doc, result);
                if (fixResult.FixedCount > 0)
                    tx.Commit();
                else
                    tx.RollBack();
                return fixResult;
            }
            catch (Exception ex)
            {
                tx.RollBack();
                return new AutoFixResult
                {
                    FailedCount = result.Violations.Count,
                    Messages = { $"Fix failed: {ex.Message}" }
                };
            }
        }

        private static AutoFixResult FixUnjoinedWalls(Document doc, RuleResult result)
        {
            var fixResult = new AutoFixResult();

            foreach (var violation in result.Violations)
            {
                try
                {
                    if (!long.TryParse(violation.ElementId, out long idVal))
                    {
                        fixResult.FailedCount++;
                        continue;
                    }

                    var elementId = RevitVersionHelper.MakeElementId(idVal);
                    var element = doc.GetElement(elementId);
                    if (element is not Wall wall)
                    {
                        fixResult.FailedCount++;
                        continue;
                    }

                    bool changed = false;
                    if (!WallUtils.IsWallJoinAllowedAtEnd(wall, 0))
                    {
                        WallUtils.AllowWallJoinAtEnd(wall, 0);
                        changed = true;
                    }
                    if (!WallUtils.IsWallJoinAllowedAtEnd(wall, 1))
                    {
                        WallUtils.AllowWallJoinAtEnd(wall, 1);
                        changed = true;
                    }

                    if (changed)
                        fixResult.FixedCount++;
                }
                catch
                {
                    fixResult.FailedCount++;
                }
            }

            fixResult.Messages.Add($"Joined {fixResult.FixedCount} wall(s)");
            return fixResult;
        }
    }
}
