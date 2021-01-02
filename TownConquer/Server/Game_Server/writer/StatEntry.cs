using System.Collections.Generic;

namespace Game_Server.writer {
    class StatEntry {
        public string name { get; set; }
        public List<double> entries { get; set; }

        public StatEntry(string _name) {
            name = _name;
            entries = new List<double>();
        }
    }
}
