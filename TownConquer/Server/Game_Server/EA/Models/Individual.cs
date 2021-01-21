using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static Game_Server.EA.EA_1_Algo;

namespace Game_Server.EA.Models {
    class Individual {
        public Genotype gene;
        public int number;
        public string name;

        public Vector3 startPos;
        public bool won;
        public double fitness;
        public bool isElite;

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
                fitness = townLifeSum + score - (timestamp.Last() / 1000);
            }
            else {
                fitness = - (townLifeSum + score) / (timestamp.Last() / 1000);
            }
        }

        public Individual Mutate(Random r, GaussDelegate gauss) {
            Dictionary<string, int> props = gene.properties;
            double mutationProbability = 1 / props.Count();
            foreach (string key in props.Keys.ToList()) {
                if (r.NextDouble() < mutationProbability) {
                    //add or substract a small amount to the value (gauss)
                    int value = (int)Math.Ceiling(props[key] * (1 + gauss(0.5) / 10));
                    props[key] = ClampValue(value, key);
                }
            }
            return this;
        }

        public Individual Recombinate(Individual partner, Random r) {
            double u = r.NextDouble() + r.NextDouble(); // random number between 0 and 2
            var ownProps = gene.properties;
            var partnerProps = partner.gene.properties;

            foreach (string key in ownProps.Keys.ToList()) {
                // Kind.Ai = u · Elter1.Ai + (1 - u) · Elter2.Ai
                int value = (int)(u * ownProps[key] + (1 - u) * partnerProps[key]);
                ownProps[key] = ClampValue(value, key);
            }
            return this;
        }

        public Individual DeepCopy() {
            Individual other = (Individual)MemberwiseClone();
            other.gene = gene.ShallowCopy();
            return other;
        }

        /// <summary>
        /// clamps all values to its valid boundries to prevent errors
        /// </summary>
        /// <param name="value">the value to be clamped</param>
        /// <param name="key">the belonging name of the value to clamp</param>
        /// <returns>a valid value</returns>
        private int ClampValue(int value, string key) {
            switch (key) {
                case "initialConquerRadius":
                    return Math.Min(Constants.MAP_HEIGHT, Math.Max(Constants.TOWN_MIN_DISTANCE, value));
                case "maxConquerRadius":
                    return Math.Min(Constants.MAP_HEIGHT, Math.Max(Constants.TOWN_MIN_DISTANCE, value));
                case "radiusExpansionStep":
                    return Math.Min(Constants.MAP_HEIGHT, Math.Max(-Constants.MAP_HEIGHT, value));
                case "attackMinLife":
                    return Math.Min(150, Math.Max(5, value));
                default:
                    return value;
            }
        }
    }
}
