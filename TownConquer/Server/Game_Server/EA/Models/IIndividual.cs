
namespace Game_Server.EA.Models {
    interface IIndividual {
        int number { get; set; }
        double fitness { get; set; }
        bool isElite { get; set; }

        void CalcFitness();
    }
}
