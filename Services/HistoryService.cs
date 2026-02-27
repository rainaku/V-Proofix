using System;
using System.Collections.Generic;
using System.Linq;

namespace VProofix.Services
{
    public class HistoryItem
    {
        public string OriginalText { get; set; }
        public string FixedText { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class HistoryService
    {
        private const int MaxHistory = 100;
        private readonly List<HistoryItem> _history = new List<HistoryItem>();
        private readonly SettingsService _settingsService;

        public HistoryService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public void AddEntry(string original, string fixedText)
        {
            if (_settingsService.CurrentSettings.PrivacyMode) return;

            _history.Insert(0, new HistoryItem
            {
                OriginalText = original,
                FixedText = fixedText,
                Timestamp = DateTime.Now
            });

            if (_history.Count > MaxHistory)
            {
                _history.RemoveAt(_history.Count - 1);
            }
        }

        public HistoryItem GetLastEntry()
        {
            return _history.FirstOrDefault();
        }

        public void Clear()
        {
            _history.Clear();
        }

        public IReadOnlyList<HistoryItem> GetHistory()
        {
            return _history.AsReadOnly();
        }
    }
}
