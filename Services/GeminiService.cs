using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VProofix.Helpers;

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
        private static readonly string[] FallbackModels = 
        { 
            "gemma-3-27b-it",
            "gemini-2.5-flash", 
            "gemini-2.5-pro",
            "gemini-2.0-flash", 
            "gemini-2.0-flash-lite", 
            "gemini-1.5-flash", 
            "gemma-3-12b-it",
            "gemma-3-4b-it"
        };

        private string GetModelDisplayName(string modelId)
        {
            if (modelId == "gemma-3-27b-it") return "Gemma 3 27B";
            if (modelId == "gemma-3-12b-it") return "Gemma 3 12B";
            if (modelId == "gemma-3-4b-it") return "Gemma 3 4B";
            if (modelId == "gemma-3-1b-it") return "Gemma 3 1B";
            if (modelId == "gemini-2.5-flash") return "Gemini 2.5 Flash";
            if (modelId == "gemini-2.5-pro") return "Gemini 2.5 Pro";
            if (modelId == "gemini-2.0-flash") return "Gemini 2.0 Flash";
            if (modelId == "gemini-2.0-flash-lite") return "Gemini 2.0 Flash Lite";
            if (modelId == "gemini-1.5-flash") return "Gemini 1.5 Flash";
            return modelId;
        }

        public async Task<string> FixGrammarAsync(string originalText, Action<string, string>? progress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(originalText))
                return string.Empty;

            var settings = _settingsService.CurrentSettings;
            string apiKey = _settingsService.GetDecryptedApiKey();

            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("API Key is missing. Please set it in Settings.");

            string prompt = $"{settings.PromptFormat}\nTarget Language: {settings.TargetLanguage}\n\nText to fix:\n{originalText}";

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                },
                generationConfig = new
                {
                    temperature = 0.1,
                    maxOutputTokens = 8192
                }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);
            
            var tryModels = new System.Collections.Generic.List<string> { settings.ModelName };
            foreach (var fallback in FallbackModels)
            {
                if (!tryModels.Contains(fallback))
                    tryModels.Add(fallback);
            }

            string? lastError = null;
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "run_log.txt");

            foreach (var model in tryModels)
            {
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                int retriesPerModel = (model == settings.ModelName) ? 2 : 1; 

                try { File.AppendAllText(logPath, $"[{DateTime.Now:T}] Trying model: {model}...\n"); } catch { }

                for (int i = 0; i < retriesPerModel; i++)
                {
                    try
                    {
                        progress?.Invoke(L.Working, L.CallingApi(GetModelDisplayName(model)));

                        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
                        
                        int totalChars = originalText.Length;
                        progress?.Invoke(L.Working, L.Fixing(totalChars));

                        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                        if (response.IsSuccessStatusCode)
                        {
                            try { File.AppendAllText(logPath, $"[{DateTime.Now:T}] Success with: {model}\n"); } catch { }
                            
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

                        string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        lastError = $"Model '{model}' failed ({response.StatusCode}): {errorContent}";
                        
                        try { File.AppendAllText(logPath, $"[{DateTime.Now:T}] Failed: {lastError}\n"); } catch { }

                        // If it's a 4xx error (except 429), usually means the model doesn't exist or is not available.
                        // Skip retries and move to next model.
                        if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests && (int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                        {
                            break; 
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        lastError = ex.Message;
                        try { File.AppendAllText(logPath, $"[{DateTime.Now:T}] Exception: {lastError}\n"); } catch { }
                    }

                    if (i < retriesPerModel - 1)
                        await Task.Delay(1000, cancellationToken);
                }
            }

            throw new Exception($"All models failed. Last error: {lastError}");
        }
    }
}
