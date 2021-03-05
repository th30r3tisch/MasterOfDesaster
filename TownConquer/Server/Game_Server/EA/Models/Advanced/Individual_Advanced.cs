using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using static Game_Server.EA.EA_2_Algo;

namespace Game_Server.EA.Models.Advanced {
    class Individual_Advanced : Individual<Genotype_Advanced> {

        public int atkScore;
        public int deffScore;
        public int suppScore;
        public double townLifeDeviation;
        public int dominance;

        public Individual_Advanced(int number) : base( number) {
            CreateGene();
        }

        public Individual_Advanced(Random r, int number) : base(number) {
            CreateGene(r);
        }

        public Individual_Advanced(List<int> supGene, List<int> offGene, List<int> deffGene, List<int> generalGene, int number) : base(number) {
            Dictionary<string, int> deffProps = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(deffGene);
            Dictionary<string, int> offProps = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(offGene);
            Dictionary<string, int> supProps = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(supGene);
            Dictionary<string, int> genProps = Genotype_Advanced.CreateProperties<PropertyNames_General>(generalGene);
            gene = new Genotype_Advanced(supProps, offProps, deffProps, genProps);
        }

        /// <summary>
        /// Calculates the Fitness of the individual
        /// </summary>
        public override void CalcFitness() {
            suppScore = (int)(100 / (townLifeDeviation + 0.1));
            fitness = atkScore + deffScore + suppScore;
        }

        public Individual_Advanced PrepareMutate(Random r, GaussDelegate gauss) {
            Mutate(r, gauss, gene.attackProperties);
            Mutate(r, gauss, gene.defensiveProperties);
            Mutate(r, gauss, gene.supportProperties);
            Mutate(r, gauss, gene.generalProperties);
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
            Individual_Advanced child = CopyIndividual();
            Recombinate(child.gene.attackProperties, partner.gene.attackProperties, r);
            Recombinate(child.gene.defensiveProperties, partner.gene.defensiveProperties, r);
            Recombinate(child.gene.supportProperties, partner.gene.supportProperties, r);
            Recombinate(child.gene.generalProperties, partner.gene.generalProperties, r);
            return child;
        }

        private void Recombinate(Dictionary<string, int> prop1, Dictionary<string, int> prop2, Random r) {
            double u = r.NextDouble() * 1.5; // random number between 0 and 1.5
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
        protected override int ClampValue(int value, string key) {
            switch (key) {
                case nameof(PropertyNames_Advanced.ConquerRadius):
                    return Math.Min(Constants.MAP_HEIGHT, Math.Max(Constants.TOWN_MIN_DISTANCE, value));
                case nameof(PropertyNames_Advanced.AttackMinLife):
                    return Math.Min(150, Math.Max(5, value));
                case nameof(PropertyNames_Advanced.SupportRadius):
                    return Math.Min(Constants.MAP_HEIGHT, Math.Max(Constants.TOWN_MIN_DISTANCE, value));
                case nameof(PropertyNames_Advanced.SupportMaxCap):
                    return Math.Min(1000, Math.Max(5, value));
                case nameof(PropertyNames_Advanced.SupportMinCap):
                    return Math.Min(500, Math.Max(5, value));
                case nameof(PropertyNames_General.SupportTownRatio):
                    return Math.Min(100, Math.Max(0, value));
                case nameof(PropertyNames_General.DeffTownRatio):
                    return Math.Min(100, Math.Max(0, value));
                case nameof(PropertyNames_General.AtkTownRatio):
                    return Math.Min(100, Math.Max(0, value));
                case nameof(PropertyNames_General.CategorisationRadius):
                    return Math.Min(Constants.MAP_HEIGHT, Math.Max(Constants.TOWN_MIN_DISTANCE, value));
                default:
                    return value;
            }
        }

        /// <summary>
        /// Creates static genes
        /// </summary>
        protected override void CreateGene() {
            Dictionary<string, int> deffProps = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(new List<int> { 2000, 10, 1000, 100, 20 });
            Dictionary<string, int> offProps = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(new List<int> { 2000, 10, 1000, 100, 20 });
            Dictionary<string, int> supProps = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(new List<int> { 2000, 10, 1000, 100, 20 });
            Dictionary<string, int> genProps = Genotype_Advanced.CreateProperties<PropertyNames_General>(new List<int> { 85, 10, 50, 1500 });
            gene = new Genotype_Advanced(supProps, offProps, deffProps, genProps);
        }

        /// <summary>
        /// Creates random genes
        /// </summary>
        /// <param name="r">Pseudo-random number generator</param>
        protected override void CreateGene(Random r) {
            gene = new Genotype_Advanced(
                CreateProps<PropertyNames_Advanced>(r), 
                CreateProps<PropertyNames_Advanced>(r), 
                CreateProps<PropertyNames_Advanced>(r), 
                CreateGeneralProps<PropertyNames_General>(r));
        }

        private Dictionary<string, int> CreateGeneralProps<T>(Random r) {
            return Genotype_Advanced.CreateProperties<T>(new List<int> {
                r.Next(0, 100),
                r.Next(0, 100),
                r.Next(0, 100),
                r.Next(Constants.TOWN_MIN_DISTANCE, Constants.MAP_HEIGHT)
            });
        }

        /// <summary>
        /// Creates random properties
        /// </summary>
        /// <param name="r">Pseudo-random number generator</param>
        private Dictionary<string, int> CreateProps<T>(Random r) {
            return Genotype_Advanced.CreateProperties<T>(new List<int> {
                r.Next(Constants.TOWN_MIN_DISTANCE, Constants.MAP_HEIGHT),
                r.Next(5, 100),
                r.Next(Constants.TOWN_MIN_DISTANCE, Constants.MAP_HEIGHT),
                r.Next(5, 100),
                r.Next(5, 100)
            });
        }

        /// <summary>
        /// Creates a deep copy of the individual
        /// </summary>
        /// <returns>The copy of the individual</returns>
        public Individual_Advanced CopyIndividual() {
            List<int> supGene = new List<int>(gene.supportProperties.Values);
            List<int> offGene = new List<int>(gene.attackProperties.Values);
            List<int> deffGene = new List<int>(gene.defensiveProperties.Values);
            List<int> generalGene = new List<int>(gene.generalProperties.Values);
            Individual_Advanced newIndividual = new Individual_Advanced(supGene, offGene, deffGene, generalGene, 0);
            return newIndividual;
        }
    }
}
