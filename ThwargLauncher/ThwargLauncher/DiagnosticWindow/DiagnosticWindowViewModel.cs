using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using CommonControls;

namespace ThwargLauncher
{
    public class DiagnosticWindowViewModel
    {
        private readonly Configurator _configurator;
        public ICommand OpenLogsCommand { get; private set; }

        public DiagnosticWindowViewModel(Configurator configurator)
        {
            _configurator = configurator;
            OpenLogsCommand = new DelegateCommand(
                    PerformOpenLogs
                );

        }
        public string DiagnosticInfo { get { return GetDiagnosticString(); } }
        private string GetDiagnosticString()
        {
            StringBuilder text = new StringBuilder();

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            text.AppendFormat("ThwargLauncher: {0} - {1}",
                assembly.GetName().Version, assembly.Location);
            text.AppendLine();

            foreach (Configurator.GameConfig config in _configurator.GetGameConfigs())
            {
                text.AppendFormat("MagFilter: {0} - {1}", config.MagFilterVersion, config.MagFilterPath);
                text.AppendLine();
            }
            return text.ToString();
        }
        private void PerformOpenLogs()
        {
            string filepath = MagFilter.FileLocations.GetRunningFolder();
            if (string.IsNullOrEmpty(filepath))
            {
                System.Windows.MessageBox.Show("Empty running folder returned from MagFilter!");
                return;
            }
            System.Diagnostics.Process.Start(filepath);
        }
    }
}
