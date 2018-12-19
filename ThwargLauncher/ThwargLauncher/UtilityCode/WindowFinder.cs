using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace ThwargUtils
{
    class WindowFinder
    {
        #region API calls
        protected delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        protected static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        protected static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        protected static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        [DllImport("user32.dll")]
        protected static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(IntPtr hwnd, String lpString);
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);
        const short SWP_NOMOVE = 0X2;
        const short SWP_NOSIZE = 1;
        const short SWP_NOZORDER = 0X4;
        const int SWP_SHOWWINDOW = 0x0040;
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        #endregion API calls

        #region Members
        private Dictionary<IntPtr, int> _windowMap = null;
        #endregion Members

        #region Methods
        public void RecordExistingWindows()
        {
            _windowMap = new Dictionary<IntPtr, int>();
            //EnumWindows(new EnumWindowsProc(RecordWindows), this);
            EnumWindows((hWnd, lParam) =>
                {
                    _windowMap[hWnd] = 1;
                    return true;
                }, IntPtr.Zero);

        }
        public IntPtr FindNewWindow(System.Text.RegularExpressions.Regex regex, int processId)
        {
            IntPtr foundWnd = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
                {
                    if (!IsWindowVisible(hWnd)) { return true; }
                    if (_windowMap.ContainsKey(hWnd)) { return true; }
                    int size = GetWindowTextLength(hWnd);
                    if (size <= 0) { return true; }
                    StringBuilder sb = new StringBuilder(size + 1);
                    GetWindowText(hWnd, sb, size + 1);
                    if (!regex.IsMatch(sb.ToString())) { return true; }
                    if (processId != 0 && GetWindowProcessId(hWnd) != processId) { return true; }
                    foundWnd = hWnd;
                    return false;
                }, IntPtr.Zero);
            return foundWnd;
        }
        public void SetWindowTitle(IntPtr hwnd, string title)
        {
            SetWindowText(hwnd, title);
        }
        public void SetWindowPosition(IntPtr hwnd, int x, int y, int cx, int cy)
        {
            int flags = SWP_NOZORDER | SWP_NOSIZE | SWP_SHOWWINDOW;
            SetWindowPos(hwnd, 0, x, y, cx, cy, flags);
        }
        public int GetWindowProcessId(IntPtr hwnd)
        {
            uint processId = 0;
            uint threadId = GetWindowThreadProcessId(hwnd, out processId);
            return (int)processId;
        }
        #endregion Methods
    }
}
