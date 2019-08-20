using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelli.WPF.Controls.DataGrid
{
    public class Enums
    {
        public enum FilterOperation
        {
            Unknown,
            Contains,
            Equals,
            StartsWith,
            EndsWith,
            GreaterThanEqual,
            LessThanEqual,
            GreaterThan,
            LessThan,
            NotEquals
        }
        public enum ColumnOption
        {
            Unknown = 0,
            AddGrouping,
            RemoveGrouping,
            PinColumn,
            UnpinColumn
        }
    }
}
