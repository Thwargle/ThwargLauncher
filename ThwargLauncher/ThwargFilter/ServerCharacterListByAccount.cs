using System;
using System.Collections.Generic;
using System.Text;

namespace ThwargFilter
{
    public class ServerCharacterListByAccount
    {
        public string ZoneId { get; set; }
        private List<Character> _characters = new List<Character>();
        public List<Character> CharacterList { get { return _characters; } set { _characters = value; }}
    }
}
