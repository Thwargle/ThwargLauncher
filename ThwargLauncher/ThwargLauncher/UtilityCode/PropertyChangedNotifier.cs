using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ThwarglePropertyExtensions
{
    public static class PropertyChangedUtil
    {
        public static void Raise(PropertyChangedEventHandler handler, object sender, string propertyName)
        {
            if (null != handler)
            {
                handler(sender, new PropertyChangedEventArgs(propertyName));
            }
        }
        public static void RaiseBeginInvoke(PropertyChangedEventHandler handler, object sender, string propertyName)
        {
            if (null != handler)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    handler(sender, new PropertyChangedEventArgs(propertyName));
                }));
            }
        }
    }
    public static class PropertyChangedUtilExtensions
    {
        public static void Raise(this PropertyChangedEventHandler handler, object sender, string propertyName)
        {
            PropertyChangedUtil.Raise(handler, sender, propertyName);
        }
        public static void RaiseBeginInvoke(this PropertyChangedEventHandler handler, object sender, string propertyName)
        {
            PropertyChangedUtil.RaiseBeginInvoke(handler, sender, propertyName);
        }
    }
}
