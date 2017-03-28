using System;
using System.ComponentModel;

namespace ThwargLauncher
{
    public class HelpWindowViewModel
    {
        public event HandleEvent OpeningSimpleLauncherEvent;
        private Configurator _configurator;

        public HelpWindowViewModel(Configurator configurator)
        {
            _configurator = configurator;
        }
        public DiagnosticWindowViewModel GetDiagnosticWindowViewModel()
        {
            return new DiagnosticWindowViewModel(_configurator);
        }
        public void DisplaySimpleLaunchWindow()
        {
            if (OpeningSimpleLauncherEvent != null)
            {
                OpeningSimpleLauncherEvent();
            }
        }
    }
}
