using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace CommandPalette.Core
{
    public class HotkeyManager : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode,
            IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        // Физические коды клавиш — НЕ зависят от раскладки!
        // C на английской = С на русской = vkCode 0x43
        // P на английской = З на русской = vkCode 0x50
        private const uint VK_C = 0x43;
        private const uint VK_P = 0x50;
        private const int SequenceMs = 600;

        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;
        private DateTime _lastC = DateTime.MinValue;
        private readonly Action _onHotkey;

        public HotkeyManager(Action onHotkey)
        {
            _onHotkey = onHotkey;
        }

        public void Register()
        {
            _proc = HookCallback;
            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                _hookId = SetWindowsHookEx(
                    WH_KEYBOARD_LL,
                    _proc,
                    GetModuleHandle(module.ModuleName),
                    0);
            }
            Debug.WriteLine($"HotkeyManager: hook = {_hookId != IntPtr.Zero}");
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 &&
                (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                var kbs = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                uint vk = kbs.vkCode;

                if (vk == VK_C)
                {
                    _lastC = DateTime.Now;
                }
                else if (vk == VK_P)
                {
                    var diff = (DateTime.Now - _lastC).TotalMilliseconds;
                    if (diff < SequenceMs && diff > 30)
                    {
                        _lastC = DateTime.MinValue;
                        System.Windows.Application.Current?.Dispatcher.BeginInvoke(
                            DispatcherPriority.Normal,
                            new Action(() => _onHotkey?.Invoke()));
                    }
                }
                else
                {
                    _lastC = DateTime.MinValue;
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }
    }
}