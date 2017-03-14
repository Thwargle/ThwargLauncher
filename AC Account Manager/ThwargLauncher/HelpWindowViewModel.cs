using System;
using System.ComponentModel;

namespace ThwargLauncher
{
    public class HelpWindowViewModel
    {
        private Configurator _configurator;
        SimpleLaunch _simpleLaunchWindow = null;

        public HelpWindowViewModel(Configurator configurator)
        {
            _configurator = configurator;
        }
        public DiagnosticWindowViewModel GetDiagnosticWindowViewModel()
        {
            return new DiagnosticWindowViewModel(_configurator);
        }
        public void DisplaySimpleLauchWindow()
        {
            if (_simpleLaunchWindow == null)
            {
                _simpleLaunchWindow = new SimpleLaunch(new SimpleLaunchWindowViewModel(_configurator));
                _simpleLaunchWindow.Closing += _simpleLaunchWindow_Closing;
            }
            _simpleLaunchWindow.Show();
        }
        void _simpleLaunchWindow_Closing(object sender, CancelEventArgs e)
        {
            _simpleLaunchWindow = null;
        }
    }
}
