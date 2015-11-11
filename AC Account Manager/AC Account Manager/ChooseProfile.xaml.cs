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

namespace AC_Account_Manager
{
    /// <summary>
    /// Interaction logic for ChooseProfile.xaml
    /// </summary>
    public partial class ChooseProfile : Window
    {
        private ChooseProfileViewModel _viewModel = null;
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
    }
}
