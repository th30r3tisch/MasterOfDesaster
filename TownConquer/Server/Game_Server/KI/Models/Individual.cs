
using System.Numerics;

namespace Game_Server.KI.Models {
    class Individual {
        public Genotype gene;
        public Result result;

        public int number;
        public string name;
        public Vector3 startPos;

        public Individual(Genotype _gene, int _number) {
            result = new Result();
            number = _number;
            gene = _gene;
        }
    }
}
