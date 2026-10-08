using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using RevitQualityChecker.Core.Models;

namespace RevitQualityChecker.RevitAdapter.ViewModels
{
    public class RuleResultViewModel : INotifyPropertyChanged
    {
        private bool _isExpanded;

        public string RuleId { get; }
        public string RuleName { get; }
        public string RuleCategory { get; }
        public string Severity { get; }
        public bool Passed { get; }
        public string StatusIcon => Passed ? "✔" : "✘";
        public int CheckedCount { get; }
        public int ViolationCount { get; }
        public string Summary => $"{CheckedCount} checked, {ViolationCount} violations";
        public ObservableCollection<ViolationViewModel> Violations { get; }
        public bool CanAutoFix => AutoFixer.CanFix(RuleId) && !Passed;
        public ICommand AutoFixCommand { get; set; }
        public RuleResult OriginalResult { get; }

        public Brush StatusBrush => Passed
            ? new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50))
            : new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));

        public string DisplaySeverity => Passed ? "Passed" : Severity;

        public Brush SeverityBrush => Passed
            ? new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50))
            : Severity switch
            {
                "Error" => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)),
                "Warning" => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)),
                _ => new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3))
            };

        public Brush SeverityBackground => Passed
            ? new SolidColorBrush(Color.FromArgb(0x30, 0x4C, 0xAF, 0x50))
            : Severity switch
            {
                "Error" => new SolidColorBrush(Color.FromArgb(0x30, 0xF4, 0x43, 0x36)),
                "Warning" => new SolidColorBrush(Color.FromArgb(0x30, 0xFF, 0x98, 0x00)),
                _ => new SolidColorBrush(Color.FromArgb(0x30, 0x21, 0x96, 0xF3))
            };

        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(); }
        }

        public bool HasViolations => ViolationCount > 0;

        public RuleResultViewModel(RuleResult r)
        {
            RuleId = r.RuleId;
            RuleName = r.RuleName;
            RuleCategory = r.RuleCategory ?? "";
            Severity = r.Severity.ToString();
            Passed = r.Passed;
            CheckedCount = r.CheckedCount;
            ViolationCount = r.Violations.Count;
            OriginalResult = r;
            Violations = new ObservableCollection<ViolationViewModel>(
                r.Violations.Select(v => new ViolationViewModel(v)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
