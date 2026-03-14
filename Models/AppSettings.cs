using System;

namespace VProofix.Models
{
    public class AppSettings
    {
        public string GeminiApiKey { get; set; } = string.Empty;
        public string ModelName { get; set; } = "gemma-3-27b-it";
        public string AppLanguage { get; set; } = "English";
        public string TargetLanguage { get; set; } = "Auto";
        public string FixHotkey { get; set; } = "Ctrl+Alt+F";
        public string PreviewHotkey { get; set; } = "Ctrl+Alt+P";
        public bool PrivacyMode { get; set; } = true;
        public bool ShowPreviewWindow { get; set; } = false;
        public string PromptFormat { get; set; } = "Fix grammar and spelling mistakes. ONLY output the corrected text. Do NOT explain. Preserve the original formatting (like markdown and newlines).";
        public string[] FallbackModels { get; set; } = 
        { 
            "gemma-3-27b-it",
            "gemini-2.0-flash", 
            "gemini-2.0-flash-lite", 
            "gemini-1.5-flash",
            "gemini-1.5-pro",
            "gemma-3-12b-it",
            "gemma-3-4b-it" 
        };
    }
}
