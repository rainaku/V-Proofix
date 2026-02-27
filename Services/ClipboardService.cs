using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace VProofix.Services
{
    public class ClipboardService
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        const int KEYEVENTF_KEYUP = 0x0002;
        const int VK_CONTROL = 0x11;
        const int VK_C = 0x43;
        const int VK_V = 0x56;

        public async Task<string> GetSelectedTextAsync()
        {
            string originalText = string.Empty;

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    // Empty clipboard
                    Clipboard.Clear();
                }
                catch { } // ignore lock exceptions for now
            });

            // Wait for user to physically release modifiers before injecting Ctrl+C
            await WaitForModifiersReleaseAsync();

            // Need to release keys & simulate Ctrl+C without blocking UI thread
            SimulateCtrlC();

            // Poll every 10ms for up to 300ms (30 tries)
            for (int i = 0; i < 30; i++)
            {
                bool hasText = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        if (Clipboard.ContainsText())
                        {
                            originalText = Clipboard.GetText();
                            if (!string.IsNullOrEmpty(originalText))
                                hasText = true;
                        }
                    }
                    catch { }
                });

                if (hasText) break;
                await Task.Delay(10);
            }

            return originalText;
        }

        public async Task ReplaceSelectedTextAsync(string newText)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    Clipboard.SetText(newText);
                }
                catch { }
            });

            await Task.Delay(10); // allow tiny delay for clipboard to settle

            // Wait for user to release keys before pasting
            await WaitForModifiersReleaseAsync();

            // Background insert
            SimulateCtrlV();
        }

        private void SimulateCtrlC()
        {
            keybd_event(0x12, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Release Alt
            keybd_event(0x10, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Release Shift
            keybd_event(0x5B, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Release LWin
            keybd_event(0x5C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Release RWin

            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
            keybd_event(VK_C, 0, 0, UIntPtr.Zero);
            keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private void SimulateCtrlV()
        {
            keybd_event(0x12, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Release Alt
            keybd_event(0x10, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Release Shift
            keybd_event(0x5B, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Release LWin
            keybd_event(0x5C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Release RWin

            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
            keybd_event(VK_V, 0, 0, UIntPtr.Zero);
            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private async Task WaitForModifiersReleaseAsync()
        {
            int maxWaitMs = 2000; // wait max 2s
            int waited = 0;
            while (waited < maxWaitMs)
            {
                bool isDown = (GetAsyncKeyState(0x11) & 0x8000) != 0 || // Ctrl
                              (GetAsyncKeyState(0x12) & 0x8000) != 0 || // Alt
                              (GetAsyncKeyState(0x10) & 0x8000) != 0 || // Shift
                              (GetAsyncKeyState(0x5B) & 0x8000) != 0 || // LWin
                              (GetAsyncKeyState(0x5C) & 0x8000) != 0;   // RWin
                
                if (!isDown) break;

                await Task.Delay(5);
                waited += 5;
            }
        }
    }
}
