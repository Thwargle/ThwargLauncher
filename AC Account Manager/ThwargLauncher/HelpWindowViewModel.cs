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
        public void DisplaySimpleLaunchWindow()
        {
            if (_simpleLaunchWindow == null)
            {
                var vmodel = SimpleLaunchWindowViewModel.CreateViewModel();
                _simpleLaunchWindow = new SimpleLaunch(vmodel);
                _simpleLaunchWindow.Closing += _simpleLaunchWindow_Closing;
                if (SimpleLauncher != null)
                    SimpleLauncher();
            }
            _simpleLaunchWindow.Show();
        }
        void _simpleLaunchWindow_Closing(object sender, CancelEventArgs e)
        {
            _simpleLaunchWindow = null;
        }

        public event HandleEvent SimpleLauncher;
    }
}
