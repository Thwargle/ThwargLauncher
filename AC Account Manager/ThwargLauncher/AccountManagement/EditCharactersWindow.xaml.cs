using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace ThwargLauncher.AccountManagement
{
    /// <summary>
    /// Interaction logic for EditCharactersWindow.xaml
    /// </summary>
    public partial class EditCharactersWindow : Window
    {
        public EditCharactersWindow(EditCharactersViewModel viewModel)
        {
            if (viewModel == null) { throw new ArgumentNullException("viewModel", "Null view model passed to EditCharactersWindow"); }

            InitializeComponent();
            
            this.DataContext = viewModel;
        }
    }
}
