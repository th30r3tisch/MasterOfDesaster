using System.Numerics;

namespace Game_Server.EA.Models {
    class Individual {
        public Genotype gene;
        public Result result;
        public int number;
        public string name;
        public Vector3 startPos;
        public bool won;

        public Individual(Genotype gene, int number) {
            result = new Result();
            this.number = number;
            this.gene = gene;
        }
    }
}
