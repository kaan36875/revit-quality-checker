using System;
using System.IO;
using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitQualityChecker.Core.Configuration;
using RevitQualityChecker.RevitAdapter.ViewModels;
using RevitQualityChecker.RevitAdapter.Views;

namespace RevitQualityChecker.RevitAdapter
{
    [Transaction(TransactionMode.Manual)]
    public class SettingsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var rulesPath = FindDefaultRulesFile();
                if (rulesPath == null)
                {
                    TaskDialog.Show("Quality Checker", "Could not find default-rules.json");
                    return Result.Failed;
                }

                var loader = new RuleLoader();
                var definition = loader.LoadFromFile(rulesPath);
                var settings = UserSettings.Load();

                var viewModel = new SettingsViewModel(definition, settings);
                var window = new SettingsWindow(viewModel);
                window.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Quality Checker - Error", ex.Message);
                message = ex.Message;
                return Result.Failed;
            }
        }

        private string FindDefaultRulesFile()
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var path = Path.Combine(assemblyDir, "rules", "default-rules.json");
            if (File.Exists(path))
                return path;
            path = Path.Combine(assemblyDir, "default-rules.json");
            if (File.Exists(path))
                return path;
            return null;
        }
    }
}
