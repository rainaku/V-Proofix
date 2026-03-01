using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VProofix.Services
{
    public class DoubleShiftService : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private const int VK_SHIFT = 0x10;
        private const int VK_LSHIFT = 0xA0;
        private const int VK_RSHIFT = 0xA1;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelKeyboardProc? _proc;

        private bool _isShiftDown = false;
        private int _shiftHitCount = 0;
        private DateTime _lastShiftUp = DateTime.MinValue;

        public event Action? OnDoubleShift;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

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

        public void Initialize()
        {
            _proc = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                IntPtr handle = GetModuleHandle(curModule.ModuleName);
                _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, handle, 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT kbdStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                int wp = (int)wParam;
                int vk = kbdStruct.vkCode;

                if (wp == WM_KEYDOWN || wp == WM_SYSKEYDOWN)
                {
                    if (vk == VK_SHIFT || vk == VK_LSHIFT || vk == VK_RSHIFT)
                    {
                        if (!_isShiftDown)
                        {
                            _isShiftDown = true;
                            if ((DateTime.Now - _lastShiftUp).TotalMilliseconds > 400) // 400ms interval for double tap
                            {
                                _shiftHitCount = 1;
                            }
                            else
                            {
                                _shiftHitCount++;
                                if (_shiftHitCount == 2)
                                {
                                    OnDoubleShift?.Invoke();
                                    _shiftHitCount = 0; // reset
                                }
                            }
                        }
                    }
                    else
                    {
                        // Any other key breaks the chain
                        _shiftHitCount = 0;
                    }
                }
                else if (wp == WM_KEYUP || wp == WM_SYSKEYUP)
                {
                    if (vk == VK_SHIFT || vk == VK_LSHIFT || vk == VK_RSHIFT)
                    {
                        _isShiftDown = false;
                        _lastShiftUp = DateTime.Now;
                    }
                    else
                    {
                        // Some other key released, break the chain
                        _shiftHitCount = 0;
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }
    }
}
