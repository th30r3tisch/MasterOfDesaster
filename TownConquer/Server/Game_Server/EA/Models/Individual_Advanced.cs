using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using static Game_Server.EA.EA_1_Algo;

namespace Game_Server.EA.Models {
    class Individual_Advanced : Individual<Genotype_Advanced> {

        public Individual_Advanced(Genotype_Advanced gene, int number) : base(gene, number) {

        }

        public override void CalcFitness() {
            throw new NotImplementedException();
        }

        public Individual_Advanced PrepareMutate(Random r, GaussDelegate gauss) {
            Mutate(r, gauss, gene.attackProperties);
            Mutate(r, gauss, gene.defensiveProperties);
            Mutate(r, gauss, gene.supportProperties);
            return this;
        }

        private void Mutate(Random r, GaussDelegate gauss, Dictionary<string, int> props) {
            double mutationProbability = 1 / props.Count();
            foreach (string key in props.Keys.ToList()) {
                if (r.NextDouble() < mutationProbability) {
                    //add or substract a small amount to the value (gauss)
                    int value = (int)Math.Ceiling(props[key] * (1 + gauss(0.5) / 10));
                    props[key] = ClampValue(value, key);
                }
            }
        }

        public Individual_Advanced PrepareRecombination(Individual_Advanced partner, Random r) {
            Recombinate(gene.attackProperties, partner.gene.attackProperties, r);
            Recombinate(gene.defensiveProperties, partner.gene.defensiveProperties, r);
            Recombinate(gene.supportProperties, partner.gene.supportProperties, r);
            return this;
        }

        private void Recombinate(Dictionary<string, int> prop1, Dictionary<string, int> prop2, Random r) {
            double u = r.NextDouble() + r.NextDouble(); // random number between 0 and 2
            foreach (string key in prop1.Keys.ToList()) {
                // Kind.Ai = u · Elter1.Ai + (1 - u) · Elter2.Ai
                int value = (int)(u * prop1[key] + (1 - u) * prop2[key]);
                prop1[key] = ClampValue(value, key);
            }
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
                case "supportRadius":
                    return Math.Min(Constants.MAP_HEIGHT, Math.Max(Constants.TOWN_MIN_DISTANCE, value));
                case "supportMaxCap":
                    return Math.Min(1000, Math.Max(5, value));
                case "supportMinCap":
                    return Math.Min(500, Math.Max(5, value));
                case "supportTownRatio":
                    return Math.Min(99, Math.Max(0, value));
                default:
                    return value;
            }
        }
    }
}
