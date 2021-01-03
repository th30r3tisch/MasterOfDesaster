
using System.Collections.Generic;

namespace Game_Server.writer {
    class KnapsackStat : StatEntry {

        public List<double> entries { get; set; }

        public KnapsackStat(string name) : base(name) {
            entries = new List<double>();
        }
    }
}
