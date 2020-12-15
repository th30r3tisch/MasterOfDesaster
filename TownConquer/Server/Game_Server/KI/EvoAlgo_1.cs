using Game_Server.KI.KnapSack;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    class EvoAlgo_1 {

        List<GameLogic> individuals;

        public EvoAlgo_1() {
            //new KnapSack_EA();
            individuals = new List<GameLogic>();
        }

        public void CreateIndividual(GameLogic logic) {
            logic.GenereateInitialMap();
            KI_base ki = new KI_Stupid(logic.world, individuals.Count, "KI" + individuals.Count, Color.FromArgb(0, 0, 0), logic);
            if (logic.kis.Count == 0) logic.CreateKis(ki);

            individuals.Add(logic);
        }

    }
}
