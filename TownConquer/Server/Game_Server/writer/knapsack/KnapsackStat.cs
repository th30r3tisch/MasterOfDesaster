using System.Collections.Generic;

namespace Game_Server.writer.knapsack {
    class KnapsackStat : StatEntry {

        public List<double> entries { get; set; }

        public KnapsackStat(string name) : base(name) {
            entries = new List<double>();
        }
    }
}
