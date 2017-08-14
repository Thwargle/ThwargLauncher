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
        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) { return; }
            var grid = sender as DataGrid;
            if (grid == null) { return; }
            if (grid.SelectedItems.Count == 0) { return; }
            var row = grid.ItemContainerGenerator.ContainerFromIndex(grid.SelectedIndex) as DataGridRow;
            if (row.IsEditing) { return; }
            var serverNames = new List<string>();
            foreach (var item in grid.SelectedItems)
            {
                var server = item as ServerModel;
                serverNames.Add(server.ServerDisplayAlias);
            }
            string msg = string.Format("Delete {0} servers: {1}?", serverNames.Count, string.Join(", ", serverNames));
            var choice = MessageBox.Show(msg, "Delete Server", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if (choice != MessageBoxResult.OK) { e.Handled = true; }
        }
    }
}
