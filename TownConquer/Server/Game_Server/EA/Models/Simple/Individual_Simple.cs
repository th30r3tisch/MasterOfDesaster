using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using static Game_Server.EA.EA_1_Algo;

namespace Game_Server.EA.Models.Simple {
    class Individual_Simple: Individual<Genotype_Simple> {
        
        public List<int> townNumberDevelopment;
        public double townLifeSum = 0;

        public Individual_Simple(int number) : base(number) {
            townNumberDevelopment = new List<int>();
            CreateGene();
        }

        public Individual_Simple(Random r, int number) : base(number) {
            townNumberDevelopment = new List<int>();
            CreateGene(r);
        }

        /// <summary>
        /// Creates static genes
        /// </summary>
        protected override void CreateGene() {
            //gene = new Genotype_Simple(new List<int> { 2853, 5, 1000, 100, 20, 85 });
            gene = new Genotype_Simple(new List<int> { 2000, 10, 1000, 100, 20, 85 });
        }

        /// <summary>
        /// Creates random genes
        /// </summary>
        /// <param name="r">Pseudo-random number generator</param>
        protected override void CreateGene(Random r) {
            gene = new Genotype_Simple(new List<int> {
                r.Next(Constants.TOWN_MIN_DISTANCE, Constants.MAP_HEIGHT),
                r.Next(5, 99),
                r.Next(Constants.TOWN_MIN_DISTANCE, Constants.MAP_HEIGHT),
                r.Next(5, 99),
                r.Next(5, 99),
                r.Next(0, 100)
            });
        }

        /// <summary>
        /// Calculates the Fitness of the individual
        /// </summary>
        public override void CalcFitness() {
            fitness = score - (timestamp.Last() / 1000);
        }

        /// <summary>
        /// Mutates the individual
        /// </summary>
        /// <param name="r">Pseudo-random number generator</param>
        /// <param name="gauss">random number based on gauss distribution</param>
        /// <returns>The mutatet individual</returns>
        public Individual_Simple Mutate(Random r, GaussDelegate gauss) {
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

        /// <summary>
        /// Recombinates the individual with a partner
        /// </summary>
        /// <param name="partner">individual to recombine with</param>
        /// <param name="r">Pseudo-random number generator</param>
        /// <returns>The recombinated individual</returns>
        public Individual_Simple Recombinate(Individual_Simple partner, Random r) {
            double u = r.NextDouble() * 1.5; // random number between 1 and 2
            var ownProps = gene.properties;
            var partnerProps = partner.gene.properties;
            foreach (string key in ownProps.Keys.ToList()) {
                // Kind.Ai = u · Elter1.Ai + (1 - u) · Elter2.Ai
                int value = (int)(u * ownProps[key] + (1 - u) * partnerProps[key]);
                ownProps[key] = ClampValue(value, key);
            }
            return this;
        }

        /// <summary>
        /// clamps all values to its valid boundries to prevent errors
        /// </summary>
        /// <param name="value">the value to be clamped</param>
        /// <param name="key">the belonging name of the value to clamp</param>
        /// <returns>a valid value</returns>
        protected override int ClampValue(int value, string key) {
            switch (key) {
                case nameof(PropertyNames_Simple.ConquerRadius):
                    return Math.Min(Constants.MAP_HEIGHT, Math.Max(Constants.TOWN_MIN_DISTANCE, value));
                case nameof(PropertyNames_Simple.AttackMinLife):
                    return Math.Min(100, Math.Max(5, value));
                case nameof(PropertyNames_Simple.SupportRadius):
                    return Math.Min(Constants.MAP_HEIGHT, Math.Max(Constants.TOWN_MIN_DISTANCE, value));
                case nameof(PropertyNames_Simple.SupportMaxCap):
                    return Math.Min(100, Math.Max(5, value));
                case nameof(PropertyNames_Simple.SupportMinCap):
                    return Math.Min(100, Math.Max(5, value));
                case nameof(PropertyNames_Simple.SupportTownRatio):
                    return Math.Min(99, Math.Max(0, value));
                default:
                    return value;
            }
        }
    }
}
