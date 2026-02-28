using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VProofix.Services
{
    public class InputBlocker : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_RBUTTONDBLCLK = 0x0206;
        private const int WM_MBUTTONDBLCLK = 0x0209;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;

        private delegate IntPtr LowLevelHookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr _keyboardHookID = IntPtr.Zero;
        private IntPtr _mouseHookID = IntPtr.Zero;
        
        private LowLevelHookProc _keyboardProc;
        private LowLevelHookProc _mouseProc;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelHookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public int pt_x;
            public int pt_y;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        public void Block()
        {
            _keyboardProc = KeyboardHookCallback;
            _mouseProc = MouseHookCallback;

            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                IntPtr handle = GetModuleHandle(curModule.ModuleName);
                _keyboardHookID = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, handle, 0);
                _mouseHookID = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, handle, 0);
            }
        }

        public void Unblock()
        {
            if (_keyboardHookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHookID);
                _keyboardHookID = IntPtr.Zero;
            }
            if (_mouseHookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHookID);
                _mouseHookID = IntPtr.Zero;
            }
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT kbdStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                
                // Allow injected events (Ctrl+C, Ctrl+V simulated by V-Proofix) to pass through
                if ((kbdStruct.flags & 0x10) != 0) // LLKHF_INJECTED
                {
                    return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
                }

                return (IntPtr)1; // Swallow all physical keyboard inputs to prevent tab switching or typing
            }
            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT mouseStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                
                // Allow injected mouse events (if any)
                if ((mouseStruct.flags & 0x01) != 0) // LLMHF_INJECTED
                {
                    return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
                }

                int msg = (int)wParam;
                if (msg == WM_LBUTTONDOWN || msg == WM_RBUTTONDOWN || msg == WM_MBUTTONDOWN ||
                    msg == WM_LBUTTONDBLCLK || msg == WM_RBUTTONDBLCLK || msg == WM_MBUTTONDBLCLK ||
                    msg == WM_MOUSEWHEEL || msg == WM_MOUSEHWHEEL)
                {
                    return (IntPtr)1; // Swallow physical mouse clicks and wheels
                }
            }
            return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            Unblock();
        }
    }
}
