using System;
namespace ThwargLauncher
{
    public class HelpWindowViewModel
    {
        private Configurator _configurator;

        public HelpWindowViewModel(Configurator configurator)
        {
            _configurator = configurator;
        }
        public DiagnosticWindowViewModel GetDiagnosticWindowViewModel()
        {
            return new DiagnosticWindowViewModel(_configurator);
        }
    }
}
