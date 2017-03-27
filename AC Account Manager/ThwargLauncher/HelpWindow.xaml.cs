using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using WindowPlacementUtil;

namespace ThwargLauncher
{
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        private HelpWindowViewModel _viewModel;
        private DiagnosticsWindow _diagnosticsWindow;

        public HelpWindow(HelpWindowViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            chkShowStartup.IsChecked = Properties.Settings.Default.ShowHelpAtStart;
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var assemblyTitle = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            this.Title = string.Format("Help - {0} {1}", assemblyTitle, version);
            ThwargLauncher.AppSettings.WpfWindowPlacementSetting.Persist(this);
        }

        private void btnDefaultPreferences_Click(object sender, RoutedEventArgs e)
        {
            string pathtoPreferences = Configuration.UserPreferencesFile;

            if (File.Exists(pathtoPreferences))
            {
                Process.Start("notepad.exe", pathtoPreferences);
            }
            else
            {
                MessageBox.Show("Your UserPreferences file is not in the default location.", "File not found.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void btnDiagnostics_Click(object sender, RoutedEventArgs e)
        {
            if (_diagnosticsWindow == null)
            {
                _diagnosticsWindow = new DiagnosticsWindow(_viewModel.GetDiagnosticWindowViewModel());
                _diagnosticsWindow.Closing += _diagnosticsWindow_Closing;
            }
            _diagnosticsWindow.Show();
        }

        void _diagnosticsWindow_Closing(object sender, CancelEventArgs e)
        {
            _diagnosticsWindow = null;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_diagnosticsWindow != null)
            {
                _diagnosticsWindow.Close();
                _diagnosticsWindow = null;
            }
            Properties.Settings.Default.ShowHelpAtStart = chkShowStartup.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

        private void btnSimpleLaunch_Click(object sender, RoutedEventArgs e)
        {
            string exepath = Properties.Settings.Default.ACLocation;
            if (!System.IO.File.Exists(exepath))
            {
                MessageBox.Show("Client exe not found: " + exepath, "Launcher configuration error");
                return;
            }
            _viewModel.DisplaySimpleLaunchWindow();
            this.Hide();
        }
    }
}
