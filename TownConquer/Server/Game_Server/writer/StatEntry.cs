using System.Collections.Generic;

namespace Game_Server.writer {
    class StatEntry {
        string name;
        public List<double> entries;

        public StatEntry(string _name) {
            name = _name;
            entries = new List<double>();
        }
    }
}
