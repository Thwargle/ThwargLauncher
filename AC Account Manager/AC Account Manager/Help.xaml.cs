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
using System.Diagnostics;
using System.IO;

namespace AC_Account_Manager
{
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>
    public partial class Help : Window
    {
        public Help()
        {
            InitializeComponent();
        }

        private void btnDefaultPreferences_Click(object sender, RoutedEventArgs e)
        {
            string pathtoPreferences = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Asheron's Call\\UserPreferences.ini";

            if (File.Exists(pathtoPreferences))
            {
                Process.Start("notepad.exe", pathtoPreferences);
            }
            else
            {
                MessageBox.Show("Your UserPreferences file is not in the default location.", "File not found.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
