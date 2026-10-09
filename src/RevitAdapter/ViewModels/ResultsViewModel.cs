using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using RevitQualityChecker.Core.Models;
using RevitQualityChecker.Reporting;

namespace RevitQualityChecker.RevitAdapter.ViewModels
{
    public class ResultsViewModel
    {
        public string ModelName { get; private set; }
        public string Timestamp { get; private set; }
        public double Score { get; private set; }
        public string ScoreText => Score.ToString("F1");
        public int TotalChecked { get; private set; }
        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }
        public int InfoCount { get; private set; }
        public int PassedCount { get; private set; }
        public ObservableCollection<RuleResultViewModel> RuleResults { get; private set; }
        public ICommand ExportHtmlCommand { get; private set; }
        public ICommand ExportBcfCommand { get; private set; }

        public Brush ScoreBrush => Score switch
        {
            >= 80 => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)),
            >= 50 => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)),
            _ => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36))
        };

        private CheckReport _report;

        public static ResultsViewModel FromReport(CheckReport report)
        {
            var vm = new ResultsViewModel
            {
                _report = report,
                ModelName = report.ModelName,
                Timestamp = report.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                Score = report.Score,
                TotalChecked = report.TotalChecked,
                ErrorCount = report.ErrorCount,
                WarningCount = report.WarningCount,
                InfoCount = report.InfoCount,
                PassedCount = report.Results.Count(r => r.Passed),
                RuleResults = new ObservableCollection<RuleResultViewModel>(
                    report.Results.Select(r => new RuleResultViewModel(r)))
            };
            vm.ExportHtmlCommand = new RelayCommand(vm.ExportHtml);
            vm.ExportBcfCommand = new RelayCommand(vm.ExportBcf);
            return vm;
        }

        private void ExportBcf()
        {
            try
            {
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"QualityReport_{SafeModelName()}_{FileTimestamp()}",
                    DefaultExt = ".bcf",
                    Filter = "BCF files (*.bcf)|*.bcf",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };
                if (dlg.ShowDialog() != true) return;

                new BcfExporter().Export(_report, dlg.FileName);
                System.Windows.MessageBox.Show($"BCF exported to:\n{dlg.FileName}", "Export Complete",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"BCF export failed: {ex.Message}", "Error");
            }
        }

        private void ExportHtml()
        {
            try
            {
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"QualityReport_{SafeModelName()}_{FileTimestamp()}",
                    DefaultExt = ".html",
                    Filter = "HTML files (*.html)|*.html",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };
                if (dlg.ShowDialog() != true) return;

                var html = new HtmlReportGenerator().Generate(_report);
                File.WriteAllText(dlg.FileName, html);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dlg.FileName,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Export failed: {ex.Message}", "Error");
            }
        }

        private string SafeModelName() => string.Join("_", ModelName.Split(Path.GetInvalidFileNameChars()));
        private static string FileTimestamp() => DateTime.Now.ToString("yyyyMMdd_HHmmss");
    }
}
