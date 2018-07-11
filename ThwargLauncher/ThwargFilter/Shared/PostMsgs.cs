using System;
using System.Windows.Forms;
using Util;

namespace KeyUtil
{
    public static class PostMsgs
    {
        // http://msdn.microsoft.com/en-us/library/dd375731%28v=vs.85%29.aspx

        private const byte VK_RETURN = 0x0D;
        private const byte VK_SHIFT = 0x10;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_PAUSE = 0x13;
        private const byte VK_SPACE = 0x20;
        private const byte VK_ESCAPE = 0x1B;

        private static byte ScanCode(char Char)
        {
            switch (char.ToLower(Char))
            {
                case 'a': return 0x1E;
                case 'b': return 0x30;
                case 'c': return 0x2E;
                case 'd': return 0x20;
                case 'e': return 0x12;
                case 'f': return 0x21;
                case 'g': return 0x22;
                case 'h': return 0x23;
                case 'i': return 0x17;
                case 'j': return 0x24;
                case 'k': return 0x25;
                case 'l': return 0x26;
                case 'm': return 0x32;
                case 'n': return 0x31;
                case 'o': return 0x18;
                case 'p': return 0x19;
                case 'q': return 0x10;
                case 'r': return 0x13;
                case 's': return 0x1F;
                case 't': return 0x14;
                case 'u': return 0x16;
                case 'v': return 0x2F;
                case 'w': return 0x11;
                case 'x': return 0x2D;
                case 'y': return 0x15;
                case 'z': return 0x2C;
                case '/': return 0x35;
                case ' ': return 0x39;
            }
            return 0;
        }
        private static byte CharCode(char Char)
        {
            switch (char.ToLower(Char))
            {
                case 'a': return 0x41;
                case 'b': return 0x42;
                case 'c': return 0x43;
                case 'd': return 0x44;
                case 'e': return 0x45;
                case 'f': return 0x46;
                case 'g': return 0x47;
                case 'h': return 0x48;
                case 'i': return 0x49;
                case 'j': return 0x4A;
                case 'k': return 0x4B;
                case 'l': return 0x4C;
                case 'm': return 0x4D;
                case 'n': return 0x4E;
                case 'o': return 0x4F;
                case 'p': return 0x50;
                case 'q': return 0x51;
                case 'r': return 0x52;
                case 's': return 0x53;
                case 't': return 0x54;
                case 'u': return 0x55;
                case 'v': return 0x56;
                case 'w': return 0x57;
                case 'x': return 0x58;
                case 'y': return 0x59;
                case 'z': return 0x5A;
                case '/': return 0xBF;
                case ' ': return 0x20;
            }
            return 0x20;
        }
        public static void SendEnter(IntPtr wnd)
        {
            User32.PostMessage(wnd, User32.WM_KEYDOWN, (IntPtr)VK_RETURN, (UIntPtr)0x001C0001);
            User32.PostMessage(wnd, User32.WM_KEYUP, (IntPtr)VK_RETURN, (UIntPtr)0xC01C0001);
        }
        public class WndTimer : Timer
        {
            public WndTimer(IntPtr wnd) { this.Wnd = wnd; }
            public IntPtr Wnd;
        }
        static void PressShift(IntPtr wnd)
        {
            User32.PostMessage(wnd, User32.WM_KEYDOWN, (IntPtr)VK_SHIFT, (UIntPtr)0x002A0001);
        }
        static void ReleaseShift(IntPtr wnd)
        {
            User32.PostMessage(wnd, User32.WM_KEYUP, (IntPtr)VK_SHIFT, (UIntPtr)0xC02A0001);
        }
        private static void SendMsgK(IntPtr wnd, char ch)
        {
            byte code = CharCode(ch);
            uint lparam = (uint)((ScanCode(ch) << 0x10) | 1);
            User32.PostMessage(wnd, User32.WM_KEYDOWN, (IntPtr)code, (UIntPtr)(lparam));
            User32.PostMessage(wnd, User32.WM_KEYUP, (IntPtr)code, (UIntPtr)(0xC0000000 | lparam));
        }
        public static void SendMsg(IntPtr wnd, string msg)
        {
            foreach (char ch in msg)
            {
                SendMsgK(wnd, ch);
            }
        }
        class extraKeyInfo
        {
            public ushort repeatCount;
            public uint scanCode;
            public uint extendedKey;
            public uint prevKeyState;
            public uint transitionState;

            public long getint()
            {
                return repeatCount | (scanCode << 16) | (extendedKey << 24) |
                    (prevKeyState << 30) | (transitionState << 31);
            }
        };
        public static void SendChar(IntPtr wnd, Char ch)
        {
            ushort temp = (ushort)User32.VkKeyScan(ch);
            byte vkCode = (byte)(0xFF & temp);
            byte comboState = (byte)(temp >> 8);
            extraKeyInfo lParam = new extraKeyInfo();
            lParam.scanCode = (char)User32.MapVirtualKey(vkCode, User32.MAPVK_VK_TO_VSC);
            lParam.repeatCount = 1;
            User32.PostMessage(wnd, User32.WM_CHAR, (IntPtr)ch, (UIntPtr)lParam.getint());
        }
        public static void SendK(IntPtr wnd, Char ch, Int32 delayMs)
        {
            ushort temp = (ushort)User32.VkKeyScan(ch);
            byte vkCode = (byte)(0xFF & temp);
            byte comboState = (byte)(temp >> 8);
            extraKeyInfo lParam = new extraKeyInfo();
            lParam.scanCode = (char)User32.MapVirtualKey(vkCode, User32.MAPVK_VK_TO_VSC);
            if (IsShift(comboState))
            {
                var array = new byte[256];
                User32.GetKeyboardState(array);
                array[VK_SHIFT] = 1; // shift
                User32.SetKeyboardState(array);
                PressShift(wnd);
            }
            User32.PostMessage(wnd, User32.WM_KEYDOWN, (IntPtr)vkCode, (UIntPtr)lParam.getint());
            lParam.repeatCount = 1;
            lParam.prevKeyState = 1;
            lParam.transitionState = 1;
            if (delayMs > 0)
            {
                System.Threading.Thread.Sleep(delayMs);
            }
            User32.PostMessage(wnd, User32.WM_KEYUP, (IntPtr)vkCode, (UIntPtr)lParam.getint());
            if (IsShift(comboState)) { ReleaseShift(wnd); }

        }
        private static bool IsShift(byte comboState)
        {
            return ((comboState & 0x01) != 0);
        }
        public static void SendCharString(IntPtr wnd, string msg)
        {
            foreach (char ch in msg)
            {
                /*
                 * For '/', ScanCode=0x35 and CharCode=0xBF
                 * But SendChar(...,'/') doesn't work; do not know why not
                 * */
                if (ch == '/' && false)
                {
                    SendMsgK(wnd, ch);
                }
                else
                {
                    SendChar(wnd, ch);
                }
            }
        }
        public static void SendMouseClick(IntPtr wnd, short x, short y)
        {
            int loc = (y << 16) | (x & 0xffff);

            User32.PostMessage(wnd, User32.WM_MOUSEMOVE, (IntPtr)0x00000000, (UIntPtr)loc);
            User32.PostMessage(wnd, User32.WM_LBUTTONDOWN, (IntPtr)0x00000001, (UIntPtr)loc);
            User32.PostMessage(wnd, User32.WM_LBUTTONUP, (IntPtr)0x00000000, (UIntPtr)loc);
        }
    }
}
