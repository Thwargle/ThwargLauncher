using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Intelli.WPF.Controls.DataGrid
{
    public class OptionColumnInfo
    {
        public DataGridColumn Column { get; set; }
        public bool IsValid { get; set; }
        public string PropertyPath { get; set; }
        public IValueConverter Converter { get; set; }
        public object ConverterParameter { get; set; }
        public System.Globalization.CultureInfo ConverterCultureInfo { get; set; }
        public Type PropertyType { get; set; }

        public OptionColumnInfo(DataGridColumn column, Type boundObjectType)
        {
            if (column == null)
                return;

            Column = column;
            var boundColumn = column as DataGridBoundColumn;
            if (boundColumn != null)
            {
                System.Windows.Data.Binding binding = boundColumn.Binding as System.Windows.Data.Binding;
                if (binding != null && !string.IsNullOrWhiteSpace(binding.Path.Path))
                {
                    System.Reflection.PropertyInfo propInfo = null;
                    if (boundObjectType != null)
                        propInfo = boundObjectType.GetProperty(binding.Path.Path);

                    if (propInfo != null)
                    {
                        IsValid = true;
                        PropertyPath = binding.Path.Path;
                        PropertyType = propInfo != null ? propInfo.PropertyType : typeof(string);
                        Converter = binding.Converter;
                        ConverterCultureInfo = binding.ConverterCulture;
                        ConverterParameter = binding.ConverterParameter;
                    }
                    else
                    {
                        if (System.Diagnostics.Debugger.IsAttached && System.Diagnostics.Debugger.IsLogging())
                            System.Diagnostics.Debug.WriteLine("Intelli.WPF.Controls.DataGrid.IntelliGrid: BindingExpression path error: '{0}' property not found on '{1}'", binding.Path.Path, boundObjectType.ToString());
                    }
                }
            }
            else if (column.SortMemberPath != null && column.SortMemberPath.Length > 0)
            {
                PropertyPath = column.SortMemberPath;
                PropertyType = boundObjectType.GetProperty(column.SortMemberPath).PropertyType;
            }
        }

        public override string ToString()
        {
            if (PropertyPath != null)
                return PropertyPath;
            else
                return string.Empty;
        }
    }
}
