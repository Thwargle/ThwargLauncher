using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        public ChooseProfile()
        {
            InitializeComponent();

            var profileMgr = new ProfileManager();
            var profiles = profileMgr.GetAllProfiles();
            _viewModel = new ChooseProfileViewModel(profiles);
            DataContext = _viewModel;
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            LoadWindowSettings();
        }
        private void LoadWindowSettings()
        {
            this.SetPlacement(Properties.Settings.Default.ChooseProfileWindowPlacement);
        }
        private void SaveWindowSettings()
        {
            Properties.Settings.Default.ChooseProfileWindowPlacement = this.GetPlacement();
            Properties.Settings.Default.Save();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowSettings();
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
    }
}
