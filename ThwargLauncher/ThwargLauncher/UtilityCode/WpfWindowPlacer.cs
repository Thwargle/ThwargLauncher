using System;
using System.Windows;
using System.Windows.Interop;

namespace WindowPlacementUtil
{
    internal static class WpfWindowPlacer
    {
        public static void SetPlacement(this Window window, string placementXml)
        {
            try
            {
                WindowPlacement.SetPlacement(new WindowInteropHelper(window).Handle, placementXml);
            }
            catch
            {
            }
        }

        public static string GetPlacement(this Window window)
        {
            return WindowPlacement.GetPlacement(new WindowInteropHelper(window).Handle);
        }
    }
}
