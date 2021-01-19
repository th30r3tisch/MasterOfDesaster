using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Game_Server.EA.Models {
    class Individual {
        public Genotype gene;
        public int number;
        public string name;

        public Vector3 startPos;
        public bool won;
        public int fitness;

        public List<int> townNumberDevelopment;
        public List<int> timestamp;
        public int score = 0;
        public int townLifeSum = 0;

        public Individual(Genotype gene, int number) {
            townNumberDevelopment = new List<int>();
            timestamp = new List<int>();
            this.number = number;
            this.gene = gene;
        }

        public void CalcFitness() {
            if (won) {
                fitness = townLifeSum + score - timestamp.Last();
            }
            else {
                fitness = - (townLifeSum + score) / timestamp.Last();
            }
        }
        public Individual DeepCopy() {
            Individual other = (Individual)MemberwiseClone();
            other.gene = gene.ShallowCopy();
            return other;
        }
    }
}
