using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ThwargLauncher
{
    /// <summary>
    /// Interaction logic for LogViewerWindow.xaml
    /// </summary>
    public partial class LogViewerWindow : Window
    {
        private string TestData = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";
        private List<string> words;
        private int maxword;
        private int index;

        public ObservableCollection<LogEntry> LogEntries { get; set; }

        public LogViewerWindow()
        {
            InitializeComponent();

            random = new Random();
            words = TestData.Split(' ').ToList();
            maxword = words.Count - 1;

            DataContext = LogEntries = new ObservableCollection<LogEntry>();
            Enumerable.Range(0, 200000)
                      .ToList()
                      .ForEach(x => LogEntries.Add(GetRandomEntry()));

            Timer = new System.Threading.Timer(x => AddRandomEntry(), null, 1000, 10);
        }

        private System.Threading.Timer Timer;
        private System.Random random;
        private void AddRandomEntry()
        {
            Dispatcher.BeginInvoke((Action)(() => LogEntries.Add(GetRandomEntry())));
        }

        private LogEntry GetRandomEntry()
        {
            if (random.Next(1, 10) > 1)
            {
                return new LogEntry()
                {
                    Index = index++,
                    DateTime = DateTime.Now,
                    Message = string.Join(" ", Enumerable.Range(5, random.Next(10, 50))
                                                         .Select(x => words[random.Next(0, maxword)])),
                };
            }

            return new CollapsibleLogEntry()
            {
                Index = index++,
                DateTime = DateTime.Now,
                Message = string.Join(" ", Enumerable.Range(5, random.Next(10, 50))
                                             .Select(x => words[random.Next(0, maxword)])),
                Contents = Enumerable.Range(5, random.Next(5, 10))
                                     .Select(i => GetRandomEntry())
                                     .ToList()
            };
        }
    }
}
