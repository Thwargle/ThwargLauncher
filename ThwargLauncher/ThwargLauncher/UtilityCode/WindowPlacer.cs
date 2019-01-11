
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace WindowPlacementUtil
{
    // RECT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
    }

    // POINT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    // WINDOWPLACEMENT stores the position, size, and state of a window
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;
    }

    public static class WindowPlacement
    {
        private static Encoding encoding = new UTF8Encoding();
        private static XmlSerializer serializer = new XmlSerializer(typeof(WINDOWPLACEMENT));

        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;

        public static void SetPlacementString(IntPtr windowHandle, string placementXml)
        {
            WINDOWPLACEMENT placement = GetPlacementFromString(placementXml);
            if (placement.length == 0) // flag for failure
            {
                return;
            }
            SetWindowPlacement(windowHandle, ref placement);
        }
        public static void SetPlacement(IntPtr windowHandle, WINDOWPLACEMENT placement)
        {
            if (placement.length > 0)
            {
                SetWindowPlacement(windowHandle, ref placement);
            }
        }
        public static WINDOWPLACEMENT GetPlacementFromString(string placementXml)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = 0; // flag for failure

            if (!string.IsNullOrEmpty(placementXml))
            {

                byte[] xmlBytes = encoding.GetBytes(placementXml);

                try
                {
                    using (MemoryStream memoryStream = new MemoryStream(xmlBytes))
                    {
                        placement = (WINDOWPLACEMENT)serializer.Deserialize(memoryStream);
                    }

                    if (!(placement.normalPosition.Top == 0 && placement.normalPosition.Bottom == 0))
                    {
                        placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                        placement.flags = 0;
                        placement.showCmd = (placement.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : placement.showCmd);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Parsing placement XML failed. Fail silently.
                }
            }
            return placement;
        }
        public static string GetPlacementString(IntPtr windowHandle)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            GetWindowPlacement(windowHandle, out placement);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
                {
                    serializer.Serialize(xmlTextWriter, placement);
                    byte[] xmlBytes = memoryStream.ToArray();
                    return encoding.GetString(xmlBytes);
                }
            }
        }
        public class PlacementInfo
        {
            public WINDOWPLACEMENT Placement;
            public string PlacementString;
            public bool IsEmpty() { return Placement.normalPosition.Top == 0 && Placement.normalPosition.Bottom == 0; }
        }
        public static PlacementInfo GetPlacementInfo(IntPtr windowHandle)
        {
            var info = new PlacementInfo();
            info.Placement = new WINDOWPLACEMENT();
            GetWindowPlacement(windowHandle, out info.Placement);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
                {
                    serializer.Serialize(xmlTextWriter, info.Placement);
                    byte[] xmlBytes = memoryStream.ToArray();
                    info.PlacementString = encoding.GetString(xmlBytes);
                }
            }
            return info;
        }
    }
}
