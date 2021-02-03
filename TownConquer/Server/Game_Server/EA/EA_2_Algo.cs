using Game_Server.EA.Models;
using Game_Server.writer.EA_2;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Game_Server.EA {
    class EA_2_Algo: EA_Base<EA_2_Writer> {

        public EA_2_Algo() : base() {
            _writer = new EA_2_Writer("EA");
        }

        protected override List<Individual> CreateOffspring(List<Individual> population) {
            throw new System.NotImplementedException();
        }

        protected override List<Individual> Evaluate(ConcurrentBag<Individual> results) {
            throw new System.NotImplementedException();
        }

        protected override ConcurrentBag<Individual> TrainKis(List<Individual> population) {
            throw new System.NotImplementedException();
        }

        protected override void WriteProtocoll(List<Individual> results) {
            throw new System.NotImplementedException();
        }
    }
}
