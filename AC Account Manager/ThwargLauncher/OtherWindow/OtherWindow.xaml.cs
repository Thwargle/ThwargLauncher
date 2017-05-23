using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
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

namespace ThwargLauncher.OtherWindow
{
    public partial class OtherWindow : Window
    {
        public OtherWindow()
        {
            InitializeComponent();
        }

        public void SetComboBox()
        {
            SoundBox.SelectedIndex = Properties.Settings.Default.SetServerSound;
        }

        private void OtherOptionsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.SetServerSound = SoundBox.SelectedIndex;
            MainWindow.OptionsOpen = false;
        }

        private void btnSoundSample_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SetServerSound = SoundBox.SelectedIndex;
            ServerModel.PlayServerSound();
        }
    }
}
