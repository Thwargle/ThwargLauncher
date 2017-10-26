using System;
using System.Runtime.Serialization;

namespace ThwargLauncher.WebService
{
    [DataContract]
    public class GameCommand
    {
        [DataMember]
        public string Command { get; set; }
    }
}
