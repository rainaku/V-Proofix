using System;
using System.Windows;
using VProofix.Models;
using VProofix.Services;

namespace VProofix.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _settingsService;

        public SettingsWindow(SettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            var s = _settingsService.CurrentSettings;
            txtApiKey.Password = _settingsService.GetDecryptedApiKey();
            txtModelName.Text = s.ModelName;
            cmbTargetLanguage.Text = s.TargetLanguage;
            txtFixHotkey.Text = s.FixHotkey;
            txtPreviewHotkey.Text = s.PreviewHotkey;
            chkShowPreview.IsChecked = s.ShowPreviewWindow;
            txtPromptFormat.Text = s.PromptFormat;
            chkPrivacyMode.IsChecked = s.PrivacyMode;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var s = _settingsService.CurrentSettings;
            s.ModelName = txtModelName.Text;
            s.TargetLanguage = cmbTargetLanguage.Text;
            s.FixHotkey = txtFixHotkey.Text;
            s.PreviewHotkey = txtPreviewHotkey.Text;
            s.ShowPreviewWindow = chkShowPreview.IsChecked ?? false;
            s.PromptFormat = txtPromptFormat.Text;
            s.PrivacyMode = chkPrivacyMode.IsChecked ?? true;

            _settingsService.SetEncryptedApiKey(txtApiKey.Password);
            _settingsService.SaveSettings();

            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
