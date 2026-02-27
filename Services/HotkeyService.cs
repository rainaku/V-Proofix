using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace VProofix.Services
{
    public class HotkeyService : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr _hWnd;
        private HwndSource _source;
        private int _currentId = 9000;
        private Dictionary<int, Action> _hotkeys = new Dictionary<int, Action>();

        public void Initialize()
        {
            HwndSourceParameters sourceParameters = new HwndSourceParameters("VProofixHotkeyWindow")
            {
                WindowStyle = 0,
                Width = 0,
                Height = 0,
                ParentWindow = new IntPtr(-3) // HWND_MESSAGE
            };
            _source = new HwndSource(sourceParameters);
            _hWnd = _source.Handle;
            _source.AddHook(HwndHook);
        }

        public bool Register(string hotkeyString, Action onTrigger)
        {
            try
            {
                ParseHotkeyString(hotkeyString, out uint modifiers, out uint key);
                int id = ++_currentId;
                if (RegisterHotKey(_hWnd, id, modifiers, key))
                {
                    _hotkeys[id] = onTrigger;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void UnregisterAll()
        {
            foreach (var key in _hotkeys.Keys)
            {
                UnregisterHotKey(_hWnd, key);
            }
            _hotkeys.Clear();
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (_hotkeys.TryGetValue(id, out Action action))
                {
                    action?.Invoke();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private void ParseHotkeyString(string hotkeyString, out uint modifiers, out uint key)
        {
            modifiers = 0;
            key = 0;
            var parts = hotkeyString.Split('+');
            foreach (var part in parts)
            {
                string p = part.Trim().ToUpper();
                if (p == "CTRL" || p == "CONTROL") modifiers |= 0x0002;
                else if (p == "ALT") modifiers |= 0x0001;
                else if (p == "SHIFT") modifiers |= 0x0004;
                else if (p == "WIN" || p == "WINDOWS") modifiers |= 0x0008;
                else
                {
                    if (Enum.TryParse<Key>(p, true, out Key wpfKey))
                    {
                        key = (uint)KeyInterop.VirtualKeyFromKey(wpfKey);
                    }
                }
            }
        }

        public void Dispose()
        {
            UnregisterAll();
            if (_source != null)
            {
                _source.RemoveHook(HwndHook);
                _source.Dispose();
                _source = null;
            }
        }
    }
}
