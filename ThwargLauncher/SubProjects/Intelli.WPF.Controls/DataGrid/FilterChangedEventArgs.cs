using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelli.WPF.Controls.DataGrid
{
    public class FilterChangedEventArgs : EventArgs
    {
        public Predicate<object> Filter { get; set; }

        public FilterChangedEventArgs(Predicate<object> p)
        {
            Filter = p;
        }
    }
}
