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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;

namespace AC_Account_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string arg1;
        public string arg2;
        public string arg3;
        private Dictionary<string, List<AccountCharacter>> _allAccountCharacters;
        
        private List<string> _Images = new List<string>();
        private System.Random _rand = new Random();

        public static string filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "UserNames.txt";
        private MainWindowViewModel _viewModel = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;

            CreateFolderForCurrentUser();

            LoadListBox();
            LoadImages();
            ChangeBackgroundImageRandomly();

            LoadAllAccountCharacters();

            if (Properties.Settings.Default.FrostfellChecked == true) rbFrostfell.SetCurrentValue(CheckBox.IsCheckedProperty, true);
            if (Properties.Settings.Default.ThistledownChecked == true) rbThistledown.SetCurrentValue(CheckBox.IsCheckedProperty, true);
            if (Properties.Settings.Default.HarvestgainChecked == true) rbHarvestgain.SetCurrentValue(CheckBox.IsCheckedProperty, true);
            if (Properties.Settings.Default.VerdantineChecked == true) rbVerdantine.SetCurrentValue(CheckBox.IsCheckedProperty, true);
            if (Properties.Settings.Default.LeafcullChecked == true) rbLeafcull.SetCurrentValue(CheckBox.IsCheckedProperty, true);
            if (Properties.Settings.Default.WintersebbChecked == true) rbWintersebb.SetCurrentValue(CheckBox.IsCheckedProperty, true);
            if (Properties.Settings.Default.MorningthawChecked == true) rbMorningthaw.SetCurrentValue(CheckBox.IsCheckedProperty, true);
            if (Properties.Settings.Default.DarktideChecked == true) rbDarktide.SetCurrentValue(CheckBox.IsCheckedProperty, true);
            if (Properties.Settings.Default.SolclaimChecked == true) rbSolclaim.SetCurrentValue(CheckBox.IsCheckedProperty, true);



            if (Properties.Settings.Default.ACLocation != "")
            {
                txtLauncherLocation.Text = Properties.Settings.Default.ACLocation;
            }

            lstUsername.SelectedIndex = Properties.Settings.Default.SelectedUser;
        }

        private void LoadAllAccountCharacters()
        {
            if (_allAccountCharacters != null) { throw new Exception("allAccountCharacters already populated"); }
            _allAccountCharacters = new Dictionary<string, List<AccountCharacter>>();
            var charMgr = MagFilter.CharacterManager.ReadCharacters();
            foreach (string key in charMgr.GetKeys())
            {
                var guys = new List<AccountCharacter>();
                foreach (var dude in charMgr.GetCharacters(key))
                {
                    AccountCharacter guy = new AccountCharacter();
                    guy.Id = dude.Id;
                    guy.Name = dude.Name;
                    guys.Add(guy);
                }
                _allAccountCharacters[key] = guys;
            }
        }

        private void LoadImages()
        {
            _Images.Clear();
            _Images.Add("acwallpaperwideaerbax.jpg");
            _Images.Add("acwallpaperwideaerfalle.jpg");
            _Images.Add("acwallpaperwideGroup.jpg");
        }

        private string PickRandomImage()
        {
            return _Images[_rand.Next(_Images.Count)];
        }

        private void ChangeBackgroundImageRandomly()
        {
            LoadImages();
            string imageName = PickRandomImage();
            //BigGrid.Background = MyTextBlock.Background;
            ImageBrush brush = new ImageBrush(
                new BitmapImage(
                    new Uri(imageName, UriKind.Relative)
                    ));
            ContentGrid.Background = brush;

            //
        }

        private void CreateFolderForCurrentUser()
        {
            string specificFolder = Configuration.GetFolderLocation();

            // Check if folder exists and if not, create it
            if (!Directory.Exists(specificFolder))
                Directory.CreateDirectory(specificFolder);

            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
        }

        public void LoadListBox()
        {
            LoadUserAccounts();
        }
        private void LoadUserAccounts()
        {
            _viewModel.KnownUserAccounts = new List<UserAccount>();
            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) { continue; }
                        string[] arr = line.Split(',');

                        var user = new UserAccount()
                        {
                            Name = arr[0],
                            Password = arr[1]
                        };
                        _viewModel.KnownUserAccounts.Add(user);
                    }
                }
            }
        }

        private void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            List<string> servers = new List<string>();

            if (rbFrostfell.IsChecked.Value == true) servers.Add("Frostfell");
            if (rbThistledown.IsChecked.Value == true) servers.Add("Thistledown");
            if (rbHarvestgain.IsChecked.Value == true) servers.Add("Harvestgain");
            if (rbVerdantine.IsChecked.Value == true) servers.Add("Verdantine");
            if (rbLeafcull.IsChecked.Value == true) servers.Add("Leafcull");
            if (rbWintersebb.IsChecked.Value == true) servers.Add("Wintersebb");
            if (rbMorningthaw.IsChecked.Value == true) servers.Add("Morningthaw");
            if (rbDarktide.IsChecked.Value == true) servers.Add("Darktide");
            if (rbSolclaim.IsChecked.Value == true) servers.Add("Solclaim");


            if (servers.Count != 0)
            {
                foreach (string server in servers)
                {
                    LaunchClients(server);
                }
            }
            else
            {
                MessageBox.Show("No server selected. Please select a server", "No server selected.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private void LaunchClients(string serverArgument)
        {
            foreach (Object selectedItem in lstUsername.SelectedItems)
            {
                //-username "MyUsername" -password "MyPassword" -w "ServerName" -2 -3
                UserAccount account = (selectedItem as UserAccount);
                if (account == null) { ShowMessage("Denied"); return; }
                arg1 = account.Name;
                arg2 = account.Password;
                arg3 = serverArgument;
                


                string genArgs = "-username " + arg1 + " -password " + arg2 + " -w " + arg3 + " -2 -3";
                string pathToFile = txtLauncherLocation.Text;
                Process runProg = new Process();
                if (arg2 == "")
                {
                    genArgs = "-username " + arg1 + " -w " + arg3 + " -3 ";
                    try
                    {
                        runProg.StartInfo.FileName = pathToFile;
                        runProg.StartInfo.Arguments = genArgs;
                        runProg.StartInfo.CreateNoWindow = true;
                        runProg.Start();
                    }
                    catch (Exception ex)
                    {

                        ShowMessage("Could not start program. Please Check your path. ", "Launcher not found.", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    ShowMessage("Multiple Logins Stopped. You don't have a password set, and multiple logins cannot continue.", "Multiple Logins Stopped.", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                }
                try
                {
                    runProg.StartInfo.FileName = pathToFile;
                    runProg.StartInfo.Arguments = genArgs;
                    runProg.StartInfo.CreateNoWindow = true;
                    runProg.Start();
                }
                catch (Exception ex)
                {
                    ShowMessage("Could not start program. Please check the path to your Asheron's Call Launcher executable. " + ex, "Launcher not found.", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }
                System.Threading.Thread.Sleep(15000);
            }
        }
        private void ShowMessage(string msg)
        {
            ShowMessage(msg, "Caption", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ShowMessage(string msg, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            MessageBox.Show(msg, caption, button, image);
        }

        private void txtLauncherLocation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = "C:\\Turbine\\Asheron's Call";

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".exe";
            dlg.Filter = "Executables (exe)|*.exe|All files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                txtLauncherLocation.Text = filename;
            }

        }

        private void AC_Account_Manager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.SelectedUser = lstUsername.SelectedIndex;
            Properties.Settings.Default.ACLocation = txtLauncherLocation.Text;
            Properties.Settings.Default.FrostfellChecked = rbFrostfell.IsChecked.Value;
            Properties.Settings.Default.ThistledownChecked = rbThistledown.IsChecked.Value;
            Properties.Settings.Default.HarvestgainChecked = rbHarvestgain.IsChecked.Value;
            Properties.Settings.Default.VerdantineChecked = rbVerdantine.IsChecked.Value;
            Properties.Settings.Default.LeafcullChecked = rbLeafcull.IsChecked.Value;
            Properties.Settings.Default.WintersebbChecked = rbWintersebb.IsChecked.Value;
            Properties.Settings.Default.MorningthawChecked = rbMorningthaw.IsChecked.Value;
            Properties.Settings.Default.DarktideChecked = rbDarktide.IsChecked.Value;
            Properties.Settings.Default.SolclaimChecked = rbSolclaim.IsChecked.Value;
            
            Properties.Settings.Default.Save();
        }

        private void btnAddUsers_Click(object sender, RoutedEventArgs e)
        {
            AddUsers add = new AddUsers();
            add.Show();
            this.Close();
        }

        private void btnOpenUsers_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("notepad.exe", filePath);
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            bool isWindowOpen = false;

            foreach (Window w in Application.Current.Windows)
            {
                {
                    if (w is Help)
                        isWindowOpen = true;
                        w.Activate();
                }
            }

            if (!isWindowOpen)
            {
                Help newwindow = new Help();
                newwindow.Show();
            }
        }
    }
}
