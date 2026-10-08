using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RevitQualityChecker.Core.Configuration;

namespace RevitQualityChecker.RevitAdapter.ViewModels
{
    public class RuleToggleViewModel : INotifyPropertyChanged
    {
        private bool _isEnabled;

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string Severity { get; }
        public string Category { get; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(); }
        }

        public RuleToggleViewModel(RuleDefinition def, bool enabled)
        {
            Id = def.Id;
            Name = def.Name;
            Description = def.Description;
            Severity = def.Severity.ToString();
            Category = def.TargetCategory ?? "All";
            _isEnabled = enabled;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _customRulesPath;
        private bool _useCustomRules;

        public ObservableCollection<RuleToggleViewModel> Rules { get; } = new();
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseCommand { get; }
        public ICommand EnableAllCommand { get; }
        public ICommand DisableAllCommand { get; }

        public bool Saved { get; private set; }

        public string CustomRulesPath
        {
            get => _customRulesPath;
            set { _customRulesPath = value; OnPropertyChanged(); }
        }

        public bool UseCustomRules
        {
            get => _useCustomRules;
            set { _useCustomRules = value; OnPropertyChanged(); }
        }

        public SettingsViewModel(RuleSetDefinition definition, UserSettings settings)
        {
            _customRulesPath = settings.CustomRulesFilePath ?? "";
            _useCustomRules = !string.IsNullOrEmpty(settings.CustomRulesFilePath);

            foreach (var rule in definition.Rules)
            {
                bool enabled = !settings.DisabledRuleIds.Contains(rule.Id);
                Rules.Add(new RuleToggleViewModel(rule, enabled));
            }

            SaveCommand = new RelayCommand(OnSave);
            CancelCommand = new RelayCommand(OnCancel);
            BrowseCommand = new RelayCommand(OnBrowse);
            EnableAllCommand = new RelayCommand(() => SetAll(true));
            DisableAllCommand = new RelayCommand(() => SetAll(false));
        }

        private void SetAll(bool enabled)
        {
            foreach (var rule in Rules)
                rule.IsEnabled = enabled;
        }

        public UserSettings ToSettings()
        {
            var settings = new UserSettings();
            foreach (var rule in Rules)
            {
                if (!rule.IsEnabled)
                    settings.DisabledRuleIds.Add(rule.Id);
            }
            if (UseCustomRules && !string.IsNullOrWhiteSpace(CustomRulesPath))
                settings.CustomRulesFilePath = CustomRulesPath;
            return settings;
        }

        private void OnSave()
        {
            var settings = ToSettings();
            settings.Save();
            Saved = true;
            CloseRequested?.Invoke();
        }

        private void OnCancel()
        {
            Saved = false;
            CloseRequested?.Invoke();
        }

        private void OnBrowse()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select Custom Rules File"
            };
            if (dialog.ShowDialog() == true)
            {
                CustomRulesPath = dialog.FileName;
                UseCustomRules = true;
            }
        }

        public event Action CloseRequested;
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
