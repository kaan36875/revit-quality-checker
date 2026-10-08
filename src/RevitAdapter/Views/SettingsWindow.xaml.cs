using System.Windows;
using RevitQualityChecker.RevitAdapter.ViewModels;

namespace RevitQualityChecker.RevitAdapter.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += Close;
        }
    }
}
