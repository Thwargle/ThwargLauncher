using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WindowPlacementUtil;

namespace ThwargLauncher
{
    /// <summary>
    /// Interaction logic for ChooseProfile.xaml
    /// </summary>
    public partial class ChooseProfile : Window
    {
        private ChooseProfileViewModel _viewModel = null;
        public string ProfileNameChosen { get; private set; }
        internal ChooseProfile(MainWindowViewModel mwvm)
        {
            InitializeComponent();

            var profileMgr = new ProfileManager();
            var profiles = profileMgr.GetAllProfiles();
            _viewModel = new ChooseProfileViewModel(mwvm, profiles);
            DataContext = _viewModel;
            ThwargLauncher.AppSettings.WpfWindowPlacementSetting.Persist(this);
        }

        private void Select_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (sender as Button);
            if (button == null) { return; }
            var profileModel = (button.DataContext as ProfileChoiceViewModel);
            if (profileModel == null) { return; }
            ProfileNameChosen = profileModel.Name;
            this.DialogResult = true;
            this.Close();
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) { return; }
            var grid = sender as DataGrid;
            if (grid == null) { return; }
            if (grid.SelectedItems.Count == 0) { return; }
            var row = grid.ItemContainerGenerator.ContainerFromIndex(grid.SelectedIndex) as DataGridRow;
            if (row.IsEditing) { return; }
            var profileNames = new List<string>();
            foreach (var item in grid.SelectedItems)
            {
                var profile = item as ProfileChoiceViewModel;
                profileNames.Add(profile.Name);
            }
            string msg = string.Format("Delete {0} profiles: {1}?", profileNames.Count, string.Join(", ", profileNames));
            var choice = MessageBox.Show(msg, "Delete Server", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if (choice != MessageBoxResult.OK) { e.Handled = true; }
        }
    }
}
