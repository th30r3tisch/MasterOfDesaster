
using Game_Server.EA.Models.Advanced;
using System.Collections.Generic;

namespace Game_Server.writer.EA_2 {
    class EA_2_Writer : StatsWriter<Individual_Advanced> {

        public EA_2_Writer(string filename) : base(filename) {
            
        }

        public override void WriteStats(List<Individual_Advanced> records) {
            throw new System.NotImplementedException();
        }
    }
}
