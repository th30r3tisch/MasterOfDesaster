using System.Collections.Generic;
using System.Numerics;

namespace Game_Server.EA.Models {
    abstract class Individual<T> where T: Gene{
        public T gene;
        public int number;
        public string name;

        public bool won;
        public double fitness;
        public Vector3 startPos;
        public bool isElite;
        public List<int> timestamp;

        public Individual(T gene, int number) {
            this.number = number;
            this.gene = gene;
            timestamp = new List<int>();
        }

        public Individual<T> DeepCopy() {
            Individual<T> other = (Individual<T>)MemberwiseClone();
            other.gene = (T)gene.ShallowCopy();
            return other;
        }

        public abstract void CalcFitness();
    }
}
