using System;
using System.Windows;
using System.Windows.Controls;
using VProofix.Models;
using VProofix.Services;
using VProofix.Helpers;

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
            UpdateUILanguage();
        }

        private void UpdateUILanguage()
        {
            this.Title = L.SettingsTitle;
            lblAppLanguage.Text = L.UILanguage;
            lblTargetLanguage.Text = L.TargetLanguageUI;
            lblFixHotkey.Text = L.FixHotkey;
            lblPreviewHotkey.Text = L.PreviewHotkey;
            lblPromptFormat.Text = L.PromptFormatUI;
            chkShowPreview.Content = L.CheckPreview;
            chkPrivacyMode.Content = L.CheckPrivacy;
            btnExit.Content = L.BtnExit;
            btnCancel.Content = L.BtnCancel;
            btnSave.Content = L.BtnSave;
        }

        private void LoadCurrentSettings()
        {
            var s = _settingsService.CurrentSettings;
            txtApiKey.Password = _settingsService.GetDecryptedApiKey();
            
            // Tìm và chọn model phù hợp trong ComboBox
            foreach (ComboBoxItem item in cmbModelName.Items)
            {
                if (item.Content.ToString() == s.ModelName)
                {
                    cmbModelName.SelectedItem = item;
                    break;
                }
            }
            if (cmbModelName.SelectedIndex == -1) cmbModelName.Text = s.ModelName; // Fallback if no match

            cmbAppLanguage.Text = s.AppLanguage;
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
            s.ModelName = (cmbModelName.SelectedItem as ComboBoxItem)?.Content.ToString() ?? cmbModelName.Text;
            s.AppLanguage = cmbAppLanguage.Text;
            L.CurrentLanguage = s.AppLanguage;
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
