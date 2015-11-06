using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AC_Account_Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += (sender, eargs)
                => HandleExcObject(eargs.ExceptionObject);

        }
        void HandleExcObject(object excObj)
        {
            var exc = excObj as Exception;
            if (exc == null)
            {
                exc = new NotSupportedException(
                    "Unhandled exception doesn't derive from System.Exception: "
                    + excObj.ToString());
            }
            HandleExc(exc);
        }
        void HandleExc(Exception exc)
        {
            Log.WriteLog("Fatal Exception: " + exc.ToString());
            MessageBox.Show("Fatal Program Error: See log file at " + Log.GetLogFilePath());
        }
    }
}
