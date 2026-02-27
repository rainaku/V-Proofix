using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace VProofix.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly SettingsService _settingsService;

        public GeminiService(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _httpClient = new HttpClient();
        }

        public async Task<string> FixGrammarAsync(string originalText, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(originalText))
                return string.Empty;

            var settings = _settingsService.CurrentSettings;
            string apiKey = _settingsService.GetDecryptedApiKey();

            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("API Key is missing. Please set it in Settings.");

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{settings.ModelName}:generateContent?key={apiKey}";

            string prompt = $"{settings.PromptFormat}\nTarget Language: {settings.TargetLanguage}\n\nText to fix:\n{originalText}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.1,
                    maxOutputTokens = 8192
                }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Setup Retry Policy
            int maxRetries = 2;
            int currentRetry = 0;

            while (true)
            {
                try
                {
                    var response = await _httpClient.PostAsync(url, content, cancellationToken);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        string errorHtml = await response.Content.ReadAsStringAsync(cancellationToken);
                        throw new Exception($"API Error ({response.StatusCode}): {errorHtml}");
                    }

                    string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                    using (JsonDocument doc = JsonDocument.Parse(responseString))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("candidates", out JsonElement candidates) && candidates.GetArrayLength() > 0)
                        {
                            var parts = candidates[0].GetProperty("content").GetProperty("parts");
                            if (parts.GetArrayLength() > 0)
                            {
                                string? fixedText = parts[0].GetProperty("text").GetString();
                                return fixedText?.Trim() ?? throw new Exception("API returned null text.");
                            }
                        }
                    }
                    throw new Exception("Unexpected API response format.");
                }
                catch (Exception ex) when (currentRetry < maxRetries && !(ex is OperationCanceledException))
                {
                    currentRetry++;
                    await Task.Delay(1000, cancellationToken); // Wait 1 second before retry
                }
            }
        }
    }
}
