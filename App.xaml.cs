using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using VProofix.Services;
using VProofix.Views;
using VProofix.ViewModels;
using System.Windows.Input;

namespace VProofix
{
    public partial class App : Application
    {
        public ICommand OpenSettingsCommand { get; private set; }
        private System.Windows.Forms.NotifyIcon _trayIcon;
        private SettingsService _settingsService;
        private HotkeyService _hotkeyService;
        private ClipboardService _clipboardService;
        private UIAutomationService _uiAutoService;
        private GeminiService _geminiService;
        private HistoryService _historyService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize Services
            _settingsService = new SettingsService();
            _clipboardService = new ClipboardService();
            _uiAutoService = new UIAutomationService();
            _geminiService = new GeminiService(_settingsService);
            _historyService = new HistoryService(_settingsService);
            _hotkeyService = new HotkeyService();

            // Initialize Hotkey Service Window Loop
            _hotkeyService.Initialize();

            // Register Hotkeys
            RegisterHotkeys();

            // Set up commands
            OpenSettingsCommand = new RelayCommand(o => Settings_Click(null, null));

            // Setup WinForms Tray Icon (Native Win32 implementation for maximum standard compatibility)
            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _trayIcon.Text = "V-Proofix - Grammar Fixer";
            _trayIcon.Visible = true;

            // Direct event bindings
            _trayIcon.MouseClick += (s, args) =>
            {
                if (args.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    Settings_Click(null, null);
                }
            };

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("Fix Now (Simulate)", null, (s, e) => FixNow_Click(null, null));
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Settings", null, (s, e) => Settings_Click(null, null));
            contextMenu.Items.Add("Exit", null, (s, e) => Exit_Click(null, null));

            _trayIcon.ContextMenuStrip = contextMenu;
        }

        private void RegisterHotkeys()
        {
            _hotkeyService.UnregisterAll();

            var settings = _settingsService.CurrentSettings;

            if (!string.IsNullOrEmpty(settings.FixHotkey))
            {
                bool fixRegistered = _hotkeyService.Register(settings.FixHotkey, async () => await ExecuteFixAsync(false));
                if (!fixRegistered)
                {
                    ShowTrayNotification("Hotkey Error", $"Failed to register Fix Hotkey: {settings.FixHotkey}", System.Windows.Forms.ToolTipIcon.Warning);
                }
            }

            if (!string.IsNullOrEmpty(settings.PreviewHotkey))
            {
                bool prevRegistered = _hotkeyService.Register(settings.PreviewHotkey, async () => await ExecuteFixAsync(true));
                if (!prevRegistered)
                {
                    ShowTrayNotification("Hotkey Error", $"Failed to register Preview Hotkey: {settings.PreviewHotkey}", System.Windows.Forms.ToolTipIcon.Warning);
                }
            }
        }

        public void ReloadHotkeys()
        {
            RegisterHotkeys();
        }

        private bool _isProcessing = false;
        private IndicatorWindow _indicatorWin;

        private async Task ExecuteFixAsync(bool forcePreview)
        {
            if (_isProcessing) return; // simple debounce
            _isProcessing = true;

            System.Media.SystemSounds.Beep.Play(); // Play sound to confirm hotkey caught

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Ensure window creates on UI thread
                _indicatorWin = new IndicatorWindow();
                _indicatorWin.ShowActivated = false; // Prevent focus stealing
                _indicatorWin.Show();
            });

            try
            {
                // Wait briefly for the UI of the target app to catch up (e.g. if Ctrl+A was pressed right before hotkey)
                await Task.Delay(50);

                string textToFix = await _clipboardService.GetSelectedTextAsync();
                if (string.IsNullOrWhiteSpace(textToFix))
                {
                    textToFix = _uiAutoService.GetTextFromFocusedElement();
                }

                if (string.IsNullOrWhiteSpace(textToFix))
                {
                    Application.Current.Dispatcher.Invoke(() => _indicatorWin?.SetStatus("No text selected"));
                    await Task.Delay(1000);
                    return;
                }

                var source = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                string fixedText = await _geminiService.FixGrammarAsync(textToFix, source.Token);

                _historyService.AddEntry(textToFix, fixedText);

                if (forcePreview || _settingsService.CurrentSettings.ShowPreviewWindow)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var previewWin = new PreviewWindow(fixedText, _clipboardService);
                        previewWin.Show();
                    });
                }
                else
                {
                    // Always Auto-replace unless preview is requested
                    await _clipboardService.ReplaceSelectedTextAsync(fixedText);
                    Application.Current.Dispatcher.Invoke(() => _indicatorWin?.SetStatus("Fixed!"));
                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => _indicatorWin?.SetStatus("Error!"));
                await Task.Delay(1500);
            }
            finally
            {
                if (_indicatorWin != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        if (_indicatorWin != null)
                        {
                            await _indicatorWin.CloseAnimatedAsync();
                            _indicatorWin = null;
                        }
                    });
                }
                _isProcessing = false;
            }
        }

        public void ShowTrayNotification(string title, string text, System.Windows.Forms.ToolTipIcon icon)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_trayIcon != null && _trayIcon.Visible)
                {
                    _trayIcon.ShowBalloonTip(3000, title, text, icon);
                }
            });
        }

        private async void FixNow_Click(object sender, RoutedEventArgs e)
        {
            // Just for testing directly from tray context menu
            await ExecuteFixAsync(false);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window win in Application.Current.Windows)
            {
                if (win is SettingsWindow)
                {
                    if (win.WindowState == WindowState.Minimized)
                        win.WindowState = WindowState.Normal;
                    win.Show();
                    win.Activate();
                    win.Topmost = true;  // Force to foreground
                    win.Topmost = false; 
                    win.Focus();
                    return;
                }
            }

            var settingsWindow = new SettingsWindow(_settingsService);
            settingsWindow.Closed += (s, args) => ReloadHotkeys();
            settingsWindow.Show();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
            _hotkeyService?.Dispose();
            Application.Current.Shutdown();
        }
    }
}
