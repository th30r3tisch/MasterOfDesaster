using System;
using System.Collections.Generic;
using System.Numerics;

namespace Game_Server.EA.Models {
    abstract class Individual<T> : IIndividual where T : Gene {
        public T gene { get; set; }
        public int number { get; set; }
        public string name;

        public bool won;
        public double fitness { get; set; }
        public Vector3 startPos;
        public bool isElite { get; set; }
        public int score = 0;
        public List<long> timestamp;

        public Individual(int number) {
            this.number = number;
            timestamp = new List<long>();
        }

        /// <summary>
        /// Creates a deep copy of the individual
        /// </summary>
        /// <returns>The copy of the individual</returns>
        public Individual<T> DeepCopy() {
            Individual<T> other = (Individual<T>)MemberwiseClone();
            other.gene = (T)gene.ShallowCopy();
            return other;
        }

        /// <summary>
        /// Calculates the Fitness of the individual
        /// </summary>
        public abstract void CalcFitness();

        /// <summary>
        /// Creates static genes
        /// </summary>
        protected abstract void CreateGene();

        /// <summary>
        /// Creates random genes
        /// </summary>
        /// <param name="r">Pseudo-random number generator</param>
        protected abstract void CreateGene(Random r);

        /// <summary>
        /// clamps all values to its valid boundries to prevent errors
        /// </summary>
        /// <param name="value">the value to be clamped</param>
        /// <param name="key">the belonging name of the value to clamp</param>
        /// <returns>a valid value</returns>
        protected abstract int ClampValue(int value, string key);
    }
}
