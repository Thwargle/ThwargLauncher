using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using WindowPlacementUtil;

namespace ThwargLauncher
{
    /// <summary>
    /// Interaction logic for AddUsers.xaml
    /// </summary>
    public partial class AddUsers : Window
    {
        private List<UserAccount> _userAccounts;
        private List<UserAccount> _accountsToAdd = new List<UserAccount>();
        public AddUsers(IEnumerable<UserAccount> userAccounts)
        {
            _userAccounts = userAccounts.ToList();
            InitializeComponent();
            txtUser1.Focus();
            ThwargLauncher.AppSettings.WpfWindowPlacementSetting.Persist(this);
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            _accountsToAdd.Clear();
            if (!ValidateAndAddUser(txtUser1, txtPassword1))
            {
                return;
            }
            if (!ValidateAndAddUser(txtUser2, txtPassword2))
            {
                return;
            }
            if (!ValidateAndAddUser(txtUser3, txtPassword3))
            {
                return;
            }
            if (!ValidateAndAddUser(txtUser4, txtPassword4))
            {
                return;
            }
            if (!ValidateAndAddUser(txtUser5, txtPassword5))
            {
                return;
            }
            
            foreach (var acct in _accountsToAdd)
            {
                _userAccounts.Add(acct);
            }
            AccountParser parser = new AccountParser();
            parser.WriteAccounts(_userAccounts);

            Close();
        }
        private bool ValidateAndAddUser(TextBox usernameBox, TextBox passwordBox)
        {
            if (string.IsNullOrWhiteSpace(usernameBox.Text))
            {
                // ones they didn't fill in
                return true;
            }
            if (!IsValidUserName(usernameBox.Text))
            {
                usernameBox.Focus();
                return false;

            }
            if (!IsValidPassword(usernameBox.Text, passwordBox.Text))
            {
                passwordBox.Focus();
                return false;

            }
            
            if (!AddUserAccount(usernameBox.Text, passwordBox.Text))
            {
                usernameBox.Focus();
                return false;
            }
            return true;
        }

        private bool IsValidUserName(string text)
        {
            if (text.Contains("!") ||
                text.Contains("@") ||
                text.Contains("#") ||
                text.Contains("$") ||
                text.Contains("%") ||
                text.Contains("^") ||
                text.Contains("&") ||
                text.Contains("*") ||
                text.Contains("(") ||
                text.Contains(")") ||
                text.Contains("=") ||
                text.Contains(".") ||
                text.Contains(",") ||
                text.Contains("<") ||
                text.Contains(">") ||
                text.Contains("?") ||
                text.Contains(";") ||
                text.Contains(":") ||
                String.IsNullOrWhiteSpace(text)
                )
            {
                var msg = string.Format("Name '{0}' contains an invalid character. Please do not use !@#$%^&*()=.,<>?;:", text);
                MessageBox.Show(msg, "Invalid name");
                return false;
            }
            return true;
        }

        private bool IsValidPassword(string name, string password)
        {
            if (String.IsNullOrWhiteSpace(password))
            {
                var msg = string.Format("Password for account '{0}' may not contain a space.", name);
                MessageBox.Show(msg, "Invalid password.");
                return false;
            }
            return true;
        }

        private bool AddUserAccount(string name, string password)
        {
            var newacct = new UserAccount(name, password);
            if (_userAccounts.Contains(newacct, new UserAcctComparer())
                || _accountsToAdd.Contains(newacct, new UserAcctComparer()))
            {
                MainWindow.ShowErrorMessage(string.Format("Duplicate account cannot be added: {0}", name));
                return false;
            }
            else
            {
                _accountsToAdd.Add(newacct);
            }
            return true;
        }

        private class UserAcctComparer : IEqualityComparer<UserAccount>
        {
            public bool Equals(UserAccount acct1, UserAccount acct2)
            {
                return acct1.Name.Trim() == acct2.Name.Trim();
            }
            public int GetHashCode(UserAccount acct)
            {
                return acct.Name.GetHashCode();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
