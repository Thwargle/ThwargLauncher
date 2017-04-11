using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ThwargLauncher.AccountManagement
{
    /// <summary>
    /// Interaction logic for EditServersWindow.xaml
    /// </summary>
    public partial class EditServersWindow : Window
    {
        private EditServersViewModel _viewModel;

        internal EditServersWindow(EditServersViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = _viewModel;
            _viewModel.CloseAction = new Action(() => this.Close());

            InitializeComponent();
            ThwargLauncher.AppSettings.WpfWindowPlacementSetting.Persist(this);
        }

        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var server = e.Row.Item as ServerModel;
            if (server != null && server.ServerSource == ServerModel.ServerSourceEnum.Published)
            {
                // Disallow editing of published servers
                //e.Cancel = true;
            }
        }
    }
}
