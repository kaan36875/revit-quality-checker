using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitQualityChecker.RevitAdapter.Compat;
using RevitQualityChecker.RevitAdapter.ViewModels;

namespace RevitQualityChecker.RevitAdapter.Views
{
    public partial class ResultsWindow : Window
    {
        private readonly UIDocument _uiDoc;

        public ResultsWindow(ResultsViewModel viewModel, UIDocument uiDoc)
        {
            InitializeComponent();
            DataContext = viewModel;
            _uiDoc = uiDoc;

            foreach (var rule in viewModel.RuleResults)
            {
                if (rule.CanAutoFix)
                    rule.AutoFixCommand = new RelayCommand(() => RunAutoFix(rule));
            }
        }

        private void RunAutoFix(RuleResultViewModel ruleVm)
        {
            var result = AutoFixer.Fix(_uiDoc.Document, ruleVm.OriginalResult);
            var msg = string.Join("\n", result.Messages);
            if (result.FailedCount > 0)
                msg += $"\n{result.FailedCount} element(s) could not be fixed.";
            TaskDialog.Show("Auto-Fix Result", msg);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ViolationRow_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.DataContext is ViolationViewModel violation)
            {
                try
                {
                    if (!long.TryParse(violation.ElementId, out long idValue))
                        return;

                    var elementId = RevitVersionHelper.MakeElementId(idValue);
                    var element = _uiDoc.Document.GetElement(elementId);
                    if (element == null)
                        return;

                    if (element is View view && !view.IsTemplate)
                    {
                        _uiDoc.ActiveView = view;
                    }
                    else
                    {
                        _uiDoc.Selection.SetElementIds(new List<ElementId> { elementId });
                        _uiDoc.ShowElements(elementId);
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Navigation Error", ex.Message);
                }
            }
        }
    }
}
