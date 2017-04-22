using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;
using System.Globalization;

namespace ThwargLauncher
{
    [ValueConversion(typeof(object), typeof(string))]
    public class ToolTipContentConverter : MarkupExtension, IValueConverter
    {
        public ToolTipContentConverter() { }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
