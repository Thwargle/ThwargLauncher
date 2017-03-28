using System;
using System.ComponentModel;

namespace ThwargLauncher
{
    public class HelpWindowViewModel
    {
        public event HandleEvent OpeningSimpleLauncherEvent;
        public event LaunchGameDelegateMethod LaunchingSimpleGameEvent;
        private Configurator _configurator;
        SimpleLaunchWindow _simpleLaunchWindow = null;

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
                _simpleLaunchWindow = new SimpleLaunchWindow(vmodel);
                _simpleLaunchWindow.Closing += _simpleLaunchWindow_Closing;
                vmodel.LaunchingEvent += _simpleLaunchWindow_LaunchingEvent;
                if (OpeningSimpleLauncherEvent != null)
                {
                    OpeningSimpleLauncherEvent();
                }
            }
            _simpleLaunchWindow.Show();
        }

        private void _simpleLaunchWindow_LaunchingEvent(LaunchItem launchItem)
        {
            if (LaunchingSimpleGameEvent == null) { throw new Exception("HelpWindowViewModel lacking implementation of LaunchingEvent"); }
            LaunchingSimpleGameEvent(launchItem);
        }

        void _simpleLaunchWindow_Closing(object sender, CancelEventArgs e)
        {
            _simpleLaunchWindow = null;
        }

    }
}
