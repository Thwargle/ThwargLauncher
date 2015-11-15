using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AC_Account_Manager
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
        public IntPtr FindNewWindow(System.Text.RegularExpressions.Regex regex)
        {
            IntPtr foundWnd = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
                {
                    if (!IsWindowVisible(hWnd)) { return true; }
                    if (_windowMap.ContainsKey(hWnd)) { return true; }
                    int size = GetWindowTextLength(hWnd);
                    if (size <= 0) { return true; }
                    StringBuilder sb = new StringBuilder(size);
                    GetWindowText(hWnd, sb, size);
                    if (!regex.IsMatch(sb.ToString())) { return true; }
                    foundWnd = hWnd;
                    return false;
                }, IntPtr.Zero);
            return foundWnd;
        }
        public void SetWindowTitle(IntPtr hwnd, string title)
        {
            SetWindowText(hwnd, title);
        }
        #endregion Methods
    }
}
