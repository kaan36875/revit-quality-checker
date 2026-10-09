using System;
using System.Reflection;
using Autodesk.Revit.UI;

namespace RevitQualityChecker.RevitAdapter
{
    public class QualityCheckerApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            var tabName = "Quality Checker";
            application.CreateRibbonTab(tabName);

            var panel = application.CreateRibbonPanel(tabName, "Quality Check");

            var assemblyPath = Assembly.GetExecutingAssembly().Location;

            var helpUrl = new ContextualHelp(ContextualHelpType.Url, "https://github.com/kaan36875/revit-quality-checker");

            var checkButton = new PushButtonData(
                "CheckModel",
                "Check\nModel",
                assemblyPath,
                typeof(QualityCheckerCommand).FullName)
            {
                ToolTip = "Run quality checks on the active model",
                LongDescription = "Validates the active Revit model against enabled quality rules including naming conventions, required parameters, and model health checks. Results are displayed in a detailed window with scoring, violation details, and export options.",
                LargeImage = RibbonIcons.CreateCheckIcon(32),
                Image = RibbonIcons.CreateCheckIcon(16)
            };
            checkButton.SetContextualHelp(helpUrl);

            var settingsButton = new PushButtonData(
                "Settings",
                "Settings",
                assemblyPath,
                typeof(SettingsCommand).FullName)
            {
                ToolTip = "Configure which rules to run and load custom rule files",
                LongDescription = "Open the settings panel to enable or disable individual quality rules and optionally load a custom JSON rules file for your project or company standards.",
                LargeImage = RibbonIcons.CreateSettingsIcon(32),
                Image = RibbonIcons.CreateSettingsIcon(16)
            };
            settingsButton.SetContextualHelp(helpUrl);

            var aboutButton = new PushButtonData(
                "About",
                "About",
                assemblyPath,
                typeof(AboutCommand).FullName)
            {
                ToolTip = "About Revit Quality Checker",
                LongDescription = "View version information, author details, and supported Revit versions.",
                LargeImage = RibbonIcons.CreateAboutIcon(32),
                Image = RibbonIcons.CreateAboutIcon(16)
            };
            aboutButton.SetContextualHelp(helpUrl);

            panel.AddItem(checkButton);
            panel.AddItem(settingsButton);
            panel.AddItem(aboutButton);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
