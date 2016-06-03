using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

/*
 * http://neilmosafi.blogspot.com/2008/07/is-inotifypropertychanged-anti-pattern.html
 * 

 Example:
 * 
public class Order : INotifyPropertyChangedPlus
{
    public Order()
    {
        PropertyDependencyManager.Register(this);
    }
    public decimal ItemPrice
    {
        get { return itemPrice; }
        set
        {
            itemPrice = value;
            OnPropertyChanged("ItemPrice");
        }
    }
    public int Quantity
    {
        get { return quantity; }
        set
        {
            quantity = value;
            OnPropertyChanged("Quantity");
        }
    }
    [DependsOn("ItemPrice", "Quantity")]
    public decimal TotalPrice
    {
        get { return ItemPrice*Quantity; }
    }
    public void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;
}


    public class SalesOrder : Order
    {
        public decimal SalesCommission
        {
            get { return salesCommission; }
            set
            {
                salesCommission = value;
                OnPropertyChanged("SalesCommission");
            }
        }
        [DependsOn("TotalPrice", "SalesCommission")]
        public decimal TotalCommission
        {
            get { return TotalPrice*SalesCommission; }
        }
    }
 
 * 
 * */
namespace PropertyDependencyUtility
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DependsOnAttribute : Attribute
    {
        public DependsOnAttribute(params string[] properties)
        {
            Properties = properties;
        }

        public string[] Properties { get; private set; }
    }
    public interface INotifyPropertyChangedPlus : INotifyPropertyChanged
    {
        void OnPropertyChanged(string propertyName);
    }
    public class PropertyDependencyManager
    {
        private static readonly List<PropertyDependencyManager> registeredInstances = new List<PropertyDependencyManager>();
        private readonly INotifyPropertyChangedPlus notifyTarget;
        private readonly Type targetType;
        private Dictionary<string, List<string>> dependencyGraph;

        private PropertyDependencyManager(INotifyPropertyChangedPlus target)
        {
            notifyTarget = target;
            targetType = target.GetType();
            notifyTarget.PropertyChanged += notifyTarget_PropertyChanged;
            CreateDependencyGraph();
        }

        public static void Register(INotifyPropertyChangedPlus target)
        {
            registeredInstances.Add(new PropertyDependencyManager(target));
        }

        private void CreateDependencyGraph()
        {
            dependencyGraph = new Dictionary<string, List<string>>();

            foreach (var property in targetType.GetProperties())
            {
                foreach (DependsOnAttribute attribute in property.GetCustomAttributes(typeof(DependsOnAttribute), true))
                {
                    foreach (var propertyWithDependee in attribute.Properties)
                    {
                        if (!dependencyGraph.ContainsKey(propertyWithDependee))
                        {
                            dependencyGraph.Add(propertyWithDependee, new List<string>());
                        }
                        dependencyGraph[propertyWithDependee].Add(property.Name);
                    }
                }
            }
        }

        private void notifyTarget_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (dependencyGraph.ContainsKey(e.PropertyName))
            {
                foreach (var dependeeProperty in dependencyGraph[e.PropertyName])
                {
                    notifyTarget.OnPropertyChanged(dependeeProperty);
                }
            }
        }
    }
}
