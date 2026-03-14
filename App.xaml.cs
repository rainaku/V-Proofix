using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using VProofix.Services;
using VProofix.Views;
using VProofix.ViewModels;
using VProofix.Helpers;
using System.Windows.Input;
using System.Net.Http;


namespace VProofix
{
    public partial class App : Application
    {
        private static Mutex? _mutex;
        private const string AppGuid = "62333969-0220-4e7b-8b92-5f8aa9b9ae8f"; // Matching conversation ID for uniqueness

        public ICommand OpenSettingsCommand { get; private set; } = null!;
        private System.Windows.Forms.NotifyIcon _trayIcon = null!;
        private SettingsService _settingsService = null!;
        private HotkeyService _hotkeyService = null!;
        private ClipboardService _clipboardService = null!;
        private UIAutomationService _uiAutoService = null!;
        private GeminiService _geminiService = null!;
        private HistoryService _historyService = null!;
        private DoubleShiftService _doubleShiftService = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, "Global\\" + AppGuid, out bool createdNew);

            if (!createdNew)
            {
                // App is already running
                MessageBox.Show("V-Proofix is already running in the system tray.", "Already Running", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);

            // Initialize Services
            _settingsService = new SettingsService();
            L.CurrentLanguage = _settingsService.CurrentSettings.AppLanguage;
            _clipboardService = new ClipboardService();
            _uiAutoService = new UIAutomationService();
            _geminiService = new GeminiService(_settingsService);
            _historyService = new HistoryService(_settingsService);
            _hotkeyService = new HotkeyService();
            _doubleShiftService = new DoubleShiftService();

            // Initialize Hotkey Service Window Loop
            _hotkeyService.Initialize();
            
            // Initialize Double Shift global hook
            _doubleShiftService.Initialize();
            _doubleShiftService.OnDoubleShift += OnDoubleShiftDetected;

            // Register Hotkeys
            RegisterHotkeys();

            // Set up commands
            OpenSettingsCommand = new RelayCommand(o => Settings_Click(new object(), new RoutedEventArgs()));

            // Setup WinForms Tray Icon (Native Win32 implementation for maximum standard compatibility)
            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "VProofix.exe");
            _trayIcon.Text = "V-Proofix - Grammar Fixer";
            _trayIcon.Visible = true;

            // Direct event bindings
            _trayIcon.MouseClick += (s, args) =>
            {
                if (args.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    Settings_Click(new object(), new RoutedEventArgs());
                }
            };

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            contextMenu.Items.Add("Fix Now (Simulate)", null, (s, e) => FixNow_Click(new object(), new RoutedEventArgs()));
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Settings", null, (s, e) => Settings_Click(new object(), new RoutedEventArgs()));
            contextMenu.Items.Add("Exit", null, (s, e) => Exit_Click(new object(), new RoutedEventArgs()));

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
            
            // Register Summary Hotkey
            bool summaryRegistered = _hotkeyService.Register("Ctrl+Alt+S", async () => await ExecuteSummaryAsync());
            if (!summaryRegistered)
            {
                ShowTrayNotification("Hotkey Error", $"Failed to register Summary Hotkey: Ctrl+Alt+S", System.Windows.Forms.ToolTipIcon.Warning);
            }
        }

        public void ReloadHotkeys()
        {
            RegisterHotkeys();
        }

        private async void OnDoubleShiftDetected()
        {
            if (_isProcessing) return;
            _isProcessing = true;

            try
            {
                await Task.Delay(50);

                string textToFix = await _clipboardService.GetSelectedTextAsync();
                if (string.IsNullOrWhiteSpace(textToFix))
                {
                    textToFix = _uiAutoService.GetTextFromFocusedElement();
                }

                if (string.IsNullOrWhiteSpace(textToFix)) 
                {
                    _isProcessing = false;
                    return;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    CaseMenuWindow? menu = null;
                    menu = new CaseMenuWindow(async (option) =>
                    {
                        try
                        {
                            if (menu != null)
                            {
                                menu = null;
                            }
                            await Task.Delay(100); // Important: Give OS time to restore focus to original app

                            string newText = textToFix;
                            if (option == "UPPER")
                                newText = textToFix.ToUpper();
                            else if (option == "LOWER")
                                newText = textToFix.ToLower();
                            else if (option == "CAPITAL")
                            {
                                if (textToFix.Length > 0)
                                    newText = char.ToUpper(textToFix[0]) + textToFix.Substring(1).ToLower();
                            }
                            else if (option == "SENTENCE")
                            {
                                if (textToFix.Length > 0)
                                {
                                    string lower = textToFix.ToLower();
                                    newText = System.Text.RegularExpressions.Regex.Replace(lower, @"(^[\s]*[a-z])|([.!?]\s+[a-z])", m => m.Value.ToUpper(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                                }
                            }

                            if (newText != textToFix)
                            {
                                await _clipboardService.ReplaceSelectedTextAsync(newText);
                            }
                        }
                        finally
                        {
                            _isProcessing = false;
                        }
                    });
                    
                    menu.Closed += (s, e) => 
                    {
                        // Fallback reset if window is closed without selection (e.g Esc or Deactivated)
                        _isProcessing = false; 
                    };
                    
                    menu.Show();
                });
            }
            catch
            {
                _isProcessing = false;
            }
        }

        private bool _isProcessing = false;
        private IndicatorWindow? _indicatorWin;

        private async Task ExecuteFixAsync(bool forcePreview)
        {
            if (_isProcessing) return; 
            _isProcessing = true;

            System.Media.SystemSounds.Beep.Play(); 

            try
            {
                await Task.Delay(50);

                string textToFix = await _clipboardService.GetSelectedTextAsync();
                if (string.IsNullOrWhiteSpace(textToFix))
                {
                    textToFix = _uiAutoService.GetTextFromFocusedElement();
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _indicatorWin = new IndicatorWindow();
                    _indicatorWin.ShowActivated = false; 
                    _indicatorWin.Show();
                });

                if (string.IsNullOrWhiteSpace(textToFix))
                {
                    Application.Current.Dispatcher.Invoke(() => _indicatorWin?.SetStatus(L.NoTextSelected, ""));
                    await Task.Delay(2000);
                    _clipboardService.RestoreOriginalClipboard();
                    return;
                }

                int wordCount = textToFix.Split(new char[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
                if (wordCount > 600)
                {
                    Application.Current.Dispatcher.Invoke(() => _indicatorWin?.SetStatus(L.TextTooLong, L.MaxWords(wordCount)));
                    await Task.Delay(2500);
                    _clipboardService.RestoreOriginalClipboard();
                    return;
                }

                Action<string, string> progressCallback = (statusText, subText) => 
                {
                    Application.Current.Dispatcher.Invoke(() => _indicatorWin?.SetStatus(statusText, subText));
                };

                var source = new CancellationTokenSource(TimeSpan.FromSeconds(20)); // Increased slightly
                string fixedText;
                
                using (var inputBlocker = new InputBlocker())
                {
                    inputBlocker.Block();
                    
                    fixedText = await _geminiService.FixGrammarAsync(textToFix, progressCallback, source.Token);
                    _historyService.AddEntry(textToFix, fixedText);

                    if (forcePreview || _settingsService.CurrentSettings.ShowPreviewWindow)
                    {
                        inputBlocker.Unblock(); 
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var previewWin = new PreviewWindow(fixedText, _clipboardService);
                            previewWin.Show();
                        });
                    }
                    else
                    {
                        await _clipboardService.ReplaceSelectedTextAsync(fixedText);
                        inputBlocker.Unblock(); 
                        Application.Current.Dispatcher.Invoke(() => _indicatorWin?.SetStatus(L.Fixed, L.Done));
                        await Task.Delay(300);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Application.Current.Dispatcher.Invoke(() => _indicatorWin?.SetStatus(L.Timeout, ""));
                await Task.Delay(2000);
                _clipboardService.RestoreOriginalClipboard();
            }
            catch (Exception ex)
            {
                string errorMsg = L.SysError;
                
                if (ex is GeminiException gex)
                {
                    if (gex.StatusCode == 401) errorMsg = L.ErrInvalidKey;
                    else if (gex.StatusCode == 429) errorMsg = L.ErrRateLimit;
                    else if (gex.StatusCode == 403) errorMsg = "API Error 403 / Access Denied";
                    else if (gex.StatusCode >= 500) errorMsg = "Provider Error / Lỗi hệ thống AI";
                    else errorMsg = $"AI Error: {gex.Message.Split('\n')[0]}";
                }
                else if (ex.Message.ToLower().Contains("api key is missing"))
                    errorMsg = L.ErrMissingKey;
                else if (ex is HttpRequestException)
                    errorMsg = "Connection Error / Lỗi kết nối máy chủ";
                else
                    errorMsg = $"Error: {ex.Message.Split('\n')[0]}";

                Application.Current.Dispatcher.Invoke(() => _indicatorWin?.SetStatus(errorMsg, ""));
                await Task.Delay(2500);
                
                _clipboardService.RestoreOriginalClipboard();
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

        private async Task ExecuteSummaryAsync()
        {
            if (_isProcessing) return; 
            _isProcessing = true;

            System.Media.SystemSounds.Beep.Play(); 

            try
            {
                await Task.Delay(50);

                string textToFix = await _clipboardService.GetSelectedTextAsync();
                if (string.IsNullOrWhiteSpace(textToFix))
                {
                    textToFix = _uiAutoService.GetTextFromFocusedElement();
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _indicatorWin = new IndicatorWindow();
                    _indicatorWin.ShowActivated = false; 
                    _indicatorWin.Show();
                });

                if (string.IsNullOrWhiteSpace(textToFix))
                {
                    Application.Current.Dispatcher.Invoke(() => _indicatorWin?.SetStatus(L.NoTextSelected, ""));
                    await Task.Delay(2000);
                    _clipboardService.RestoreOriginalClipboard();
                    return;
                }

                Action<string, string> progressCallback = (statusText, subText) => 
                {
                    Application.Current.Dispatcher.Invoke(() => _indicatorWin?.SetStatus(statusText, subText));
                };

                var source = new CancellationTokenSource(TimeSpan.FromSeconds(25));
                string summaryText;
                
                using (var inputBlocker = new InputBlocker())
                {
                    inputBlocker.Block();
                    
                    summaryText = await _geminiService.SummarizeTextAsync(textToFix, progressCallback, source.Token);

                    inputBlocker.Unblock(); 

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var summaryWin = new SummaryWindow(summaryText);
                        summaryWin.Show();
                    });
                }
            }
            catch (OperationCanceledException)
            {
                Application.Current.Dispatcher.Invoke(() => _indicatorWin?.SetStatus(L.Timeout, ""));
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                string errorMsg = L.SysError;
                
                if (ex is GeminiException gex)
                {
                    if (gex.StatusCode == 401) errorMsg = L.ErrInvalidKey;
                    else if (gex.StatusCode == 429) errorMsg = L.ErrRateLimit;
                    else if (gex.StatusCode == 403) errorMsg = "API Error 403 / Access Denied";
                    else if (gex.StatusCode >= 500) errorMsg = "Provider Error / Lỗi hệ thống AI";
                    else errorMsg = $"AI Error: {gex.Message.Split('\n')[0]}";
                }
                else if (ex.Message.ToLower().Contains("api key is missing"))
                    errorMsg = L.ErrMissingKey;
                else if (ex is HttpRequestException)
                    errorMsg = "Connection Error / Lỗi kết nối máy chủ";
                else
                    errorMsg = $"Error: {ex.Message.Split('\n')[0]}";

                Application.Current.Dispatcher.Invoke(() => _indicatorWin?.SetStatus(errorMsg, ""));
                await Task.Delay(2500);
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
                _clipboardService.RestoreOriginalClipboard();
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
            _doubleShiftService?.Dispose();
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            Application.Current.Shutdown();
        }
    }
}
