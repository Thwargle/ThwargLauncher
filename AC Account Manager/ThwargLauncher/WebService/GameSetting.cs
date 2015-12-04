using System;
using System.Runtime.Serialization;
using System.Text;


namespace ThwargLauncher.WebService
{
    [DataContract]
    public class GameSetting
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Value { get; set; }
    }
}
