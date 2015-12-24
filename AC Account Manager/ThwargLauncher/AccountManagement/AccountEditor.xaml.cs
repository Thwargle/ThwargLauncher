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
using WindowPlacementUtil;

namespace ThwargLauncher.AccountManagement
{
    /// <summary>
    /// Interaction logic for AccountEditor.xaml
    /// </summary>
    public partial class AccountEditor : Window
    {
        public AccountEditor()
        {
            InitializeComponent();
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            LoadWindowSettings();
        }
        private void LoadWindowSettings()
        {
            this.SetPlacement(Properties.Settings.Default.AccountEditorWindowPlacement);
        }
        private void SaveWindowSettings()
        {
            Properties.Settings.Default.AccountEditorWindowPlacement = this.GetPlacement();
            Properties.Settings.Default.Save();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowSettings();
        }
        private void WriteAccountSettings()
        {
            AccountEditorViewModel vm = (this.DataContext as AccountEditorViewModel);
            vm.StoreToDisk();
        }
        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            WriteAccountSettings();
            Close();
        }
    }
}
