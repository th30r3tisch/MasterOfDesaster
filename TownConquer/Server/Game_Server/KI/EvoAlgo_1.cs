using System.Collections.Generic;
using System.Drawing;

namespace Game_Server.KI {
    class EvoAlgo_1 {

        List<GameLogic> individuals;
        const int populationNumber = 10;

        public EvoAlgo_1() {
            
            CreatePopulation();
            
            StatsWriter writer = new StatsWriter("EA");
        }

        private void CreatePopulation() {
            individuals = new List<GameLogic>();
            int populationCount = 0;
            while (populationCount < populationNumber) {
                GameLogic eaLogic = new GameLogic();
                CreateIndividual(eaLogic);
                populationCount++;
            }

        }

        private void CreateIndividual(GameLogic logic) {
            logic.GenereateInitialMap();
            KI_base ki = new KI_Stupid(logic.world, individuals.Count, "KI" + individuals.Count, Color.FromArgb(0, 0, 0), logic);
            if (logic.kis.Count == 0) logic.CreateKis(ki);

            individuals.Add(logic);
        }

    }
}
