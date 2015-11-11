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
    }
}
