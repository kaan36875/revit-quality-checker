using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitQualityChecker.Core.Configuration;
using RevitQualityChecker.Core.Engine;
using RevitQualityChecker.RevitAdapter.ViewModels;
using RevitQualityChecker.RevitAdapter.Views;

namespace RevitQualityChecker.RevitAdapter
{
    [Transaction(TransactionMode.Manual)]
    public class QualityCheckerCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var doc = commandData.Application.ActiveUIDocument?.Document;
                if (doc == null)
                {
                    TaskDialog.Show("Quality Checker", "No document is open.");
                    return Result.Cancelled;
                }

                var modelName = !string.IsNullOrEmpty(doc.PathName)
                    ? Path.GetFileName(doc.PathName)
                    : doc.Title;

                var settings = UserSettings.Load();

                var rulesPath = FindRulesFile(doc, settings);
                if (rulesPath == null)
                {
                    TaskDialog.Show("Quality Checker", "Could not find default-rules.json");
                    return Result.Failed;
                }

                var progress = new ProgressWindow();
                progress.Show();

                try
                {
                    progress.UpdateStatus("Loading rules...");
                    var loader = new RuleLoader();
                    var definition = loader.LoadFromFile(rulesPath);

                    if (settings.DisabledRuleIds.Count > 0)
                        definition.Rules.RemoveAll(r => settings.DisabledRuleIds.Contains(r.Id));

                    var rules = loader.CreateRules(definition);

                    progress.UpdateStatus("Collecting elements...", $"{definition.Rules.Count} rules loaded");
                    var collector = new RevitElementCollector();
                    var elementInfos = collector.CollectElements(doc);

                    progress.UpdateStatus("Running quality checks...", $"{elementInfos.Count} elements found");
                    var engine = new RuleEngine();
                    engine.AddRules(rules);
                    var report = engine.Run(modelName, elementInfos);

                    progress.Close();

                    var uiDoc = commandData.Application.ActiveUIDocument;
                    var viewModel = ResultsViewModel.FromReport(report);
                    var window = new ResultsWindow(viewModel, uiDoc);
                    window.ShowDialog();
                }
                catch
                {
                    progress.Close();
                    throw;
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Quality Checker - Error", ex.Message);
                message = ex.Message;
                return Result.Failed;
            }
        }

        private string FindRulesFile(Document doc, UserSettings settings)
        {
            if (!string.IsNullOrEmpty(settings.CustomRulesFilePath) && File.Exists(settings.CustomRulesFilePath))
                return settings.CustomRulesFilePath;

            if (!string.IsNullOrEmpty(doc.PathName))
            {
                var projectRules = Path.Combine(Path.GetDirectoryName(doc.PathName)!, "quality-rules.json");
                if (File.Exists(projectRules))
                    return projectRules;
            }

            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var defaultRules = Path.Combine(assemblyDir, "rules", "default-rules.json");
            if (File.Exists(defaultRules))
                return defaultRules;

            defaultRules = Path.Combine(assemblyDir, "default-rules.json");
            if (File.Exists(defaultRules))
                return defaultRules;

            return null;
        }
    }
}
