using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.IO;

namespace AC_Account_Manager
{
    /// <summary>
    /// Interaction logic for AddUsers.xaml
    /// </summary>
    public partial class AddUsers : Window
    {
        public AddUsers()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            using (StreamWriter writer = File.AppendText(MainWindow.filePath))
            {
                if(txtUser1.Text != "")
                    writer.WriteLine(txtUser1.Text + "," + txtPassword1.Text);

                if (txtUser2.Text != "")
                    writer.WriteLine(txtUser2.Text + "," + txtPassword2.Text);

                if (txtUser3.Text != "")
                    writer.WriteLine(txtUser3.Text + "," + txtPassword3.Text);

                if (txtUser4.Text != "")
                    writer.WriteLine(txtUser4.Text + "," + txtPassword4.Text);

                if (txtUser5.Text != "")
                    writer.WriteLine(txtUser5.Text + "," + txtPassword5.Text);
            }

            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.LoadListBox();
            main.Show();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
