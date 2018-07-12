using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistenceHelper
{
    public interface ISettings
    {
        string GetString(string key, string defval = "");
        void SetString(string key, string value);
        bool GetBool(string key, bool defval = false);
        void SetBool(string key, bool value);
        int GetInt(string key, int defval = 0);
        void SetInt(string key, int value);
        void Save();
    }
    class SettingsFactory
    {
        private static AppSettings _Global = null;
        public static ISettings Get(string filename = "")
        {
            if (_Global == null) { _Global = new AppSettings(filename); }
            return _Global;
        }
    }
    public enum StorageFormat  { XML, JSON };
    class AppSettings : ISettings
    {
        private StorageFormat _storageFormat = StorageFormat.XML;
        private string _filepath;
        private Dictionary<string, object> _values = null;

        public AppSettings(string filename)
        {
            InitFilename(filename);
        }
        private void InitFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                string folder = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                filename = System.IO.Path.Combine(folder, "Settings");
            }
            if (string.IsNullOrEmpty(System.IO.Path.GetExtension(filename)))
            {
                string extension = (_storageFormat == StorageFormat.JSON ? ".json" : ".xml" );
                filename += extension;
            }
            string directory = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _filepath = System.IO.Path.Combine(directory, filename);
        }

        public string GetString(string key, string defval = "")
        {
            InitializeIfNeeded();
            if (!_values.ContainsKey(key)) { return defval; }
            return ObjToString(_values[key]);
        }
        public void SetString(string key, string value)
        {
            InitializeIfNeeded();
            _values[key] = value;
        }
        public bool GetBool(string key, bool defval = false)
        {
            InitializeIfNeeded();
            if (!_values.ContainsKey(key)) { return defval; }
            return ObjToBool(_values[key], defval);
        }
        public void SetBool(string key, bool value)
        {
            InitializeIfNeeded();
            _values[key] = BoolToString(value);
        }
        public int GetInt(string key, int defval = 0)
        {
            InitializeIfNeeded();
            if (!_values.ContainsKey(key)) { return defval; }
            return ObjToInt(_values[key], defval);
        }
        public void SetInt(string key, int value)
        {
            InitializeIfNeeded();
            _values[key] = IntToString(value);
        }
        private void InitializeIfNeeded()
        {
            if (_values == null) { Load(); }
        }
        private string ObjToString(object obj)
        {
            if (obj == null)
            {
                return (string)obj;
            }
            else
            {
                return obj.ToString();
            }
        }
        public static bool ObjToBool(object obj, bool defval)
        {
            if (obj == null) { return defval; }
            string text = obj.ToString();
            if (EqStr(text, "True") || EqStr(text, "Yes")) { return true; }
            if (EqStr(text, "False") || EqStr(text, "No")) { return false; }
            bool bval = defval;
            bool.TryParse(text, out bval);
            return bval;
        }
        private int ObjToInt(object obj, int defval)
        {
            if (obj == null) { return defval; }
            string text = obj.ToString();
            int value = defval;
            int.TryParse(text, out value);
            return value;
        }
        private string BoolToString(bool value)
        {
            return (value ? "True" : "False");
        }
        private string IntToString(int value)
        {
            return value.ToString();
        }
        private static bool EqStr(string text1, string text2)
        {
            return (string.Compare(text1, text2, StringComparison.InvariantCultureIgnoreCase) == 0);
        }
        public void Load()
        {
            _values = new Dictionary<string, object>();
            if (System.IO.File.Exists(_filepath))
            {
                try
                {
                    string text =  System.IO.File.ReadAllText(_filepath);
                    var data = SerializeAllValuesFromString(text);
                    if (data != null)
                    {
                        _values = data;
                    }
                }
                catch (Exception exc)
                {
                    string debug = exc.ToString();
                }
            }
        }
        private Dictionary<string, object> SerializeAllValuesFromString(string text)
        {
            if (_storageFormat == StorageFormat.JSON)
            {
                object obj = Procurios.Public.JSON.JsonDecode(text);
                if (obj is Dictionary<string, object>)
                {
                    return obj as Dictionary<string, object>;
                }
                else
                {
                    return null;
                }
            }
            else if (_storageFormat == StorageFormat.XML)
            {
                var settings = new Dictionary<string, object>();
                using (var txtrdr = new System.IO.StringReader(text))
                {
                    var doc = System.Xml.Linq.XDocument.Load(txtrdr);
                    var m = doc.Root;
                    foreach (var elem in m.Elements())
                    {
                        System.Xml.Linq.XNamespace x = elem.Name.Namespace; // "http://schemas.microsoft.com/2003/10/Serialization/Arrays";
                        string key = elem.Element(x + "Key").Value;
                        string value = elem.Element(x + "Value").Value;
                        // Handle objects encoded in value?
                        settings[key] = value;
                    }
                    return settings;
                }
                /*
                 * Easy method but no handling for nested objects
                var data = System.Xml.Linq.XElement.Parse(text)
                    .Elements("setting")
                    .ToDictionary(
                        el => (string)el.Attribute("key"),
                        el => (object)el.Value
                    );
                return data;
                 * */
            }
            else
            {
                return null;
            }
        }
        public void Save()
        {
            string text = SerializeAllValuesToString();
            if (text != null)
            {
                string folder = System.IO.Path.GetDirectoryName(_filepath);
                System.IO.Directory.CreateDirectory(folder);
                System.IO.File.WriteAllText(_filepath, text);
            }
        }
        private string SerializeAllValuesToString()
        {
            if (_storageFormat == StorageFormat.JSON)
            {
                string json = Procurios.Public.JSON.JsonEncode(_values);
                return json;
            }
            else if (_storageFormat == StorageFormat.XML)
            {
                var serializer = new System.Runtime.Serialization.DataContractSerializer(_values.GetType());

                using (var sw = new System.IO.StringWriter())
                {
                    using (var writer = new System.Xml.XmlTextWriter(sw))
                    {
                        writer.Formatting = System.Xml.Formatting.Indented; // to make it easier to read
                        serializer.WriteObject(writer, _values);
                        writer.Flush();
                        return sw.ToString();
                    }
                }
            }
            else
            {
                return null;
            }
        }
    }
}
