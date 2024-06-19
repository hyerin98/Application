using System;
using System.Runtime.InteropServices;

namespace Imfine
{
    internal class PreventKey
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x100;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc = hookProc2;
        private static IntPtr hhook = IntPtr.Zero;

        public PreventKey()
        {
            //어플리케이션이 완전히 로드 된 뒤 호출 할것
            SetHook();
        }

        private void SetHook()
        {
            IntPtr hInstance = LoadLibrary("User32");
            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, hInstance, 0);
        }

        public static void UnHook()
        {
            UnhookWindowsHookEx(hhook);
        }

        public static IntPtr hookProc2(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                //vkCode 확인하기
                //TraceBox.Log("vkCode: " + vkCode);

                //91이 WINDOWS 키 이다.
                if (vkCode == 91 || vkCode == 95 || vkCode == 44)
                {
                    //해당 키를 통과 시키지 않는다.
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(hhook, code, (int)wParam, lParam);
        }
    }
}