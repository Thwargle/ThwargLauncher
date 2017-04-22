using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TwMoch.Framework
{
    /// <summary>
    /// Base class that implements property getting & setting & change notifictions through INotifyPropertyChanged
    /// Based on implementation by Julien DeGroote (TiMoch)
    /// ref: http://timoch.com/blog/2013/08/annoyed-with-inotifypropertychange/
    /// and IsNotifying idea from Caliburn-Micro
    /// ref: https://github.com/Caliburn-Micro/Caliburn.Micro/blob/master/src/Caliburn.Micro/PropertyChangedBase.cs
    /// </summary>
    public class Bindable : INotifyPropertyChanged
    {
        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        public Bindable()
        {
            IsNotifying = true;
        }

        /// <summary>
        /// To notify that all bindings should be refreshed
        /// </summary>
        public void Refresh()
        {
            NotifyOfPropertyChange(string.Empty);
        }

        /// <summary>
        /// Gets the value of a property
        /// </summary>
        protected T Get<T>([CallerMemberName] string name = null)
        {
            Debug.Assert(name != null, "name != null");
            object value = null;
            if (_properties.TryGetValue(name, out value))
                return value == null ? default(T) : (T)value;
            return default(T);
        }

        /// <summary>
        /// Sets the value of a property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <remarks>Use this overload when implicitly naming the property</remarks>
        protected bool Set<T>(T value, [CallerMemberName] string name = null)
        {
            Debug.Assert(name != null, "name != null");
            if (Equals(value, Get<T>(name)))
                return false;
            _properties[name] = value;
            NotifyOfPropertyChange(name);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsNotifying { get; set; }

        public void NotifyOfPropertyChange([CallerMemberName] string propertyName = null)
        {
            if (IsNotifying && PropertyChanged != null)
            {
                OnPropertyChanged(propertyName);
            }
        }

        protected virtual void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
