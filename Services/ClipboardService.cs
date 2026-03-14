using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace VProofix.Services
{
    public class ClipboardService
    {
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        const uint INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const ushort VK_CONTROL = 0x11;
        const ushort VK_C = 0x43;
        const ushort VK_V = 0x56;

        private IDataObject? _originalData;

        public async Task<string> GetSelectedTextAsync()
        {
            string grabbedText = string.Empty;

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    // Backup original clipboard before we hijack it
                    _originalData = Clipboard.GetDataObject();
                    Clipboard.Clear();
                }
                catch { } 
            });

            await WaitForModifiersReleaseAsync();

            // Background insert
            SimulateCtrlC();

            // Wait up to 500ms for text to arrive
            for (int i = 0; i < 50; i++)
            {
                bool hasText = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        if (Clipboard.ContainsText())
                        {
                            grabbedText = Clipboard.GetText();
                            if (!string.IsNullOrEmpty(grabbedText))
                                hasText = true;
                        }
                    }
                    catch { }
                });

                if (hasText) break;
                await Task.Delay(10);
            }

            return grabbedText;
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

            await Task.Delay(20); 

            await WaitForModifiersReleaseAsync();

            SimulateCtrlV();
            
            // Allow some time for the OS to process the paste before we (optionally) restore the clipboard
            await Task.Delay(100);
            
            RestoreOriginalClipboard();
        }

        public void RestoreOriginalClipboard()
        {
            if (_originalData == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    Clipboard.SetDataObject(_originalData, true);
                    _originalData = null; // Clear so we don't restore twice
                }
                catch { }
            });
        }

        private void SimulateCtrlC()
        {
            SendInputs(new ushort[] { 0x12, 0x10, 0x5B, 0x5C }, true); // Release Alt, Shift, LWin, RWin
            
            var inputs = new INPUT[]
            {
                CreateKeyInput(VK_CONTROL, 0),
                CreateKeyInput(VK_C, 0),
                CreateKeyInput(VK_C, KEYEVENTF_KEYUP),
                CreateKeyInput(VK_CONTROL, KEYEVENTF_KEYUP)
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private void SimulateCtrlV()
        {
            SendInputs(new ushort[] { 0x12, 0x10, 0x5B, 0x5C }, true); // Release modifiers
            
            var inputs = new INPUT[]
            {
                CreateKeyInput(VK_CONTROL, 0),
                CreateKeyInput(VK_V, 0),
                CreateKeyInput(VK_V, KEYEVENTF_KEYUP),
                CreateKeyInput(VK_CONTROL, KEYEVENTF_KEYUP)
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private void SendInputs(ushort[] keys, bool isKeyUp)
        {
            var inputs = new INPUT[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                inputs[i] = CreateKeyInput(keys[i], isKeyUp ? KEYEVENTF_KEYUP : 0);
            }
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private INPUT CreateKeyInput(ushort wVk, uint dwFlags)
        {
            return new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = wVk,
                        wScan = 0,
                        dwFlags = dwFlags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
        }

        private async Task WaitForModifiersReleaseAsync()
        {
            int maxWaitMs = 1500;
            int waited = 0;
            while (waited < maxWaitMs)
            {
                bool isDown = (GetAsyncKeyState(0x11) & 0x8000) != 0 || // Ctrl
                              (GetAsyncKeyState(0x12) & 0x8000) != 0 || // Alt
                              (GetAsyncKeyState(0x10) & 0x8000) != 0 || // Shift
                              (GetAsyncKeyState(0x5B) & 0x8000) != 0 || // LWin
                              (GetAsyncKeyState(0x5C) & 0x8000) != 0;   // RWin
                
                if (!isDown) break;
                await Task.Delay(10);
                waited += 10;
            }
        }
    }
}
