using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using WindowPlacementUtil;

namespace ThwargLauncher.AppSettings
{
    class WpfWindowPlacementSetting
    {
        public static void Persist(Window window, string key=null)
        {
            if (string.IsNullOrEmpty(key))
            {
                var st = new System.Diagnostics.StackFrame(1);
                var classname = st.GetMethod().DeclaringType.Name;
                key = classname + "Placement";
            }
            var settings = PersistenceHelper.SettingsFactory.Get();
            window.SourceInitialized += (sender, e) => { LoadPlacement(settings, window, key); };
            window.Closing += (sender, e) => { SavePlacement(settings, window, key); };
        }
        static void LoadPlacement(PersistenceHelper.ISettings settings, Window window, string key)
        {
            window.SetPlacement(settings.GetString(key, null));
        }
        static void SavePlacement(PersistenceHelper.ISettings settings, Window window, string key)
        {
            settings.SetString(key, window.GetPlacement());
            settings.Save();
        }
    }
}
