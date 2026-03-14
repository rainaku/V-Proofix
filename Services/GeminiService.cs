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

            string targetLangStr = settings.TargetLanguage == "Auto" 
                ? "the same language as the original text" 
                : settings.TargetLanguage;
            string prompt = $"{settings.PromptFormat}\nTarget Language: {targetLangStr}\n\nText to fix:\n{originalText}";

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
            if (settings.FallbackModels != null)
            {
                foreach (var fallback in settings.FallbackModels)
                {
                    if (!tryModels.Contains(fallback))
                        tryModels.Add(fallback);
                }
            }

            string? lastError = null;
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "run_log.txt");

            foreach (var model in tryModels)
            {
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                int retriesPerModel = (model == settings.ModelName) ? 2 : 1; 

                _ = Task.Run(() => { try { File.AppendAllText(logPath, $"[{DateTime.Now:T}] Trying model: {model}...\n"); } catch { } });

                for (int i = 0; i < retriesPerModel; i++)
                {
                    try
                    {
                        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                        var request = new HttpRequestMessage(HttpMethod.Post, url) 
                        { 
                            Content = content,
                            Version = new Version(2, 0)
                        };
                        
                        int totalChars = originalText.Length;
                        progress?.Invoke(L.Working, L.Fixing(totalChars));

                        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                        if (response.IsSuccessStatusCode)
                        {
                            _ = Task.Run(() => { try { File.AppendAllText(logPath, $"[{DateTime.Now:T}] Success with: {model}\n"); } catch { } });
                            
                            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                            using JsonDocument doc = await JsonDocument.ParseAsync(stream, default, cancellationToken);
                            
                            var root = doc.RootElement;
                            if (root.TryGetProperty("candidates", out JsonElement candidates) && candidates.GetArrayLength() > 0)
                            {
                                var parts = candidates[0].GetProperty("content").GetProperty("parts");
                                if (parts.GetArrayLength() > 0)
                                {
                                    string? fixedText = parts[0].GetProperty("text").GetString();
                                    return fixedText?.Trim() ?? throw new GeminiException("API returned null text.", 0);
                                }
                            }
                            
                            throw new GeminiException("Unexpected API response format.", (int)response.StatusCode);
                        }

                        string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        lastError = $"Model '{model}' failed ({response.StatusCode}): {errorContent}";
                        _ = Task.Run(() => { try { File.AppendAllText(logPath, $"[{DateTime.Now:T}] Failed: {lastError}\n"); } catch { } });

                        // If it's a client error (other than 429), don't bother trying other models or retrying
                        if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests && (int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                        {
                            throw new GeminiException(errorContent, (int)response.StatusCode); 
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (GeminiException) { throw; }
                    catch (Exception ex)
                    {
                        lastError = ex.Message;
                        _ = Task.Run(() => { try { File.AppendAllText(logPath, $"[{DateTime.Now:T}] Exception: {lastError}\n"); } catch { } });
                    }

                    if (i < retriesPerModel - 1)
                        await Task.Delay(1000, cancellationToken);
                }
            }

            throw new GeminiException($"All models failed. Last error: {lastError}", 500);
        }

        public async Task<string> SummarizeTextAsync(string originalText, Action<string, string>? progress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(originalText))
                return string.Empty;

            var settings = _settingsService.CurrentSettings;
            string apiKey = _settingsService.GetDecryptedApiKey();

            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("API Key is missing. Please set it in Settings.");

            string targetLangStr = settings.TargetLanguage == "Auto" 
                ? "the same language as the original text" 
                : settings.TargetLanguage;
            string prompt = $"You are a helpful assistant. Please summarize the following text comprehensively. Do not exceed 1000 words. Respond in the following target language: {targetLangStr}\n\nText to summarize:\n{originalText}";

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                },
                generationConfig = new
                {
                    temperature = 0.3, // Slightly higher for summarization, but still focused
                    maxOutputTokens = 8192
                }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);
            
            var tryModels = new System.Collections.Generic.List<string> { settings.ModelName };
            if (settings.FallbackModels != null)
            {
                foreach (var fallback in settings.FallbackModels)
                {
                    if (!tryModels.Contains(fallback))
                        tryModels.Add(fallback);
                }
            }

            string? lastError = null;
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "run_log.txt");

            foreach (var model in tryModels)
            {
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                int retriesPerModel = (model == settings.ModelName) ? 2 : 1; 

                _ = Task.Run(() => { try { File.AppendAllText(logPath, $"[{DateTime.Now:T}] Trying model for summary: {model}...\n"); } catch { } });

                for (int i = 0; i < retriesPerModel; i++)
                {
                    try
                    {
                        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                        var request = new HttpRequestMessage(HttpMethod.Post, url) 
                        { 
                            Content = content,
                            Version = new Version(2, 0)
                        };
                        
                        int totalChars = originalText.Length;
                        progress?.Invoke(L.Working, "Summarizing text...");

                        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                        if (response.IsSuccessStatusCode)
                        {
                            _ = Task.Run(() => { try { File.AppendAllText(logPath, $"[{DateTime.Now:T}] Success with summary on: {model}\n"); } catch { } });
                            
                            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                            using JsonDocument doc = await JsonDocument.ParseAsync(stream, default, cancellationToken);
                            
                            var root = doc.RootElement;
                            if (root.TryGetProperty("candidates", out JsonElement candidates) && candidates.GetArrayLength() > 0)
                            {
                                var parts = candidates[0].GetProperty("content").GetProperty("parts");
                                if (parts.GetArrayLength() > 0)
                                {
                                    string? summaryText = parts[0].GetProperty("text").GetString();
                                    return summaryText?.Trim() ?? throw new GeminiException("API returned null text.", 0);
                                }
                            }
                            
                            throw new GeminiException("Unexpected API response format.", (int)response.StatusCode);
                        }

                        string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        lastError = $"Model '{model}' failed ({response.StatusCode}): {errorContent}";
                        _ = Task.Run(() => { try { File.AppendAllText(logPath, $"[{DateTime.Now:T}] Failed: {lastError}\n"); } catch { } });

                        if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests && (int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                        {
                            throw new GeminiException(errorContent, (int)response.StatusCode); 
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (GeminiException) { throw; }
                    catch (Exception ex)
                    {
                        lastError = ex.Message;
                        _ = Task.Run(() => { try { File.AppendAllText(logPath, $"[{DateTime.Now:T}] Exception: {lastError}\n"); } catch { } });
                    }

                    if (i < retriesPerModel - 1)
                        await Task.Delay(1000, cancellationToken);
                }
            }

            throw new GeminiException($"All models failed for summary. Last error: {lastError}", 500);
        }
    }

    public class GeminiException : Exception
    {
        public int StatusCode { get; }
        public GeminiException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
