using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThwargLauncher
{
    public class DiagnosticWindowViewModel
    {
        private readonly Configurator _configurator;

        public DiagnosticWindowViewModel(Configurator configurator)
        {
            _configurator = configurator;
        }
        public string DiagnosticInfo { get { return GetDiagnosticString(); } }
        private string GetDiagnosticString()
        {
            StringBuilder text = new StringBuilder();

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            text.AppendFormat("ThwargLauncher: {0} - {1}",
                assembly.GetName().Version, assembly.Location);

            foreach (Configurator.GameConfig config in _configurator.GetGameConfigs())
            {
                text.AppendFormat("MagFilter: {0} - {1}\n", config.MagFilterVersion, config.MagFilterPath);
            }
            return text.ToString();
        }
    }
}
