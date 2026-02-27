using System;
using System.IO;
using System.Text.Json;
using VProofix.Models;
using VProofix.Helpers;

namespace VProofix.Services
{
    public class SettingsService
    {
        private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        public AppSettings CurrentSettings { get; private set; }

        public SettingsService()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            if (File.Exists(SettingsPath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsPath);
                    CurrentSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                    
                    if (CurrentSettings.ModelName == "gemini-1.5-flash")
                    {
                        CurrentSettings.ModelName = "gemini-2.0-flash";
                        SaveSettings();
                    }
                }
                catch
                {
                    CurrentSettings = new AppSettings();
                }
            }
            else
            {
                CurrentSettings = new AppSettings();
                SaveSettings();
            }
        }

        public void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(CurrentSettings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                // TODO: Log error
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public string GetDecryptedApiKey()
        {
            return SecurityHelper.Unprotect(CurrentSettings.GeminiApiKey);
        }

        public void SetEncryptedApiKey(string apiKey)
        {
            CurrentSettings.GeminiApiKey = SecurityHelper.Protect(apiKey);
            SaveSettings();
        }
    }
}
