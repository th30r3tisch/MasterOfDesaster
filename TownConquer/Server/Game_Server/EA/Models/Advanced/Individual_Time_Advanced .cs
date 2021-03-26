using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using static Game_Server.EA.EA_3_Algo;

namespace Game_Server.EA.Models.Advanced {
    class Individual_Time_Advanced : Individual<Genotype_Advanced> {

        public double atkScore;
        public double deffScore;
        public double suppScore;
        public double townLifeDeviation;
        public Genotype_Advanced geneEndTime;

        public int dominanceLevel = 0;
        public List<Individual_Time_Advanced> dominates = new List<Individual_Time_Advanced>();
        public int dominatedByIndividuals;
        public double crowdingDistance;

        public Individual_Time_Advanced(int number) : base( number) {
            CreateGene();
        }

        public Individual_Time_Advanced(Random r, int number) : base(number) {
            CreateGene(r);
        }

        public Individual_Time_Advanced(List<int> supGene, List<int> offGene, List<int> deffGene, List<int> generalGene, List<int> supGene2, List<int> offGene2, List<int> deffGene2, List<int> generalGene2, int number) : base(number) {
            Dictionary<string, int> deffProps = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(deffGene);
            Dictionary<string, int> offProps = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(offGene);
            Dictionary<string, int> supProps = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(supGene);
            Dictionary<string, int> genProps = Genotype_Advanced.CreateProperties<PropertyNames_General>(generalGene);
            Dictionary<string, int> deffProps2 = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(deffGene2);
            Dictionary<string, int> offProps2 = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(offGene2);
            Dictionary<string, int> supProps2 = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(supGene2);
            Dictionary<string, int> genProps2 = Genotype_Advanced.CreateProperties<PropertyNames_General>(generalGene2);
            gene = new Genotype_Advanced(supProps, offProps, deffProps, genProps);
            geneEndTime = new Genotype_Advanced(supProps2, offProps2, deffProps2, genProps2);
        }

        /// <summary>
        /// Calculates the Fitness of the individual
        /// </summary>
        public override void CalcFitness() {

            if (won) {
                suppScore = (100 / (townLifeDeviation + 0.1)) + 100;
                atkScore = (atkScore / attackActions) + 100;
                deffScore = 100 - deffScore;
            }
            else {
                suppScore = Math.Min(supportActions, 50);
                atkScore = Math.Min(attackActions, 50);
                deffScore = -deffScore;
            }
            fitness = atkScore + deffScore + suppScore;
        }

        public Individual_Time_Advanced PrepareMutate(Random r, GaussDelegate gauss) {
            Mutate(r, gauss, gene.attackProperties);
            Mutate(r, gauss, gene.defensiveProperties);
            Mutate(r, gauss, gene.supportProperties);
            Mutate(r, gauss, gene.generalProperties);
            Mutate(r, gauss, geneEndTime.attackProperties);
            Mutate(r, gauss, geneEndTime.defensiveProperties);
            Mutate(r, gauss, geneEndTime.supportProperties);
            Mutate(r, gauss, geneEndTime.generalProperties);
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

        public Individual_Time_Advanced PrepareRecombination(Individual_Time_Advanced partner, Random r) {
            Individual_Time_Advanced child = CopyIndividual();
            Recombinate(child.gene.attackProperties, partner.gene.attackProperties, r);
            Recombinate(child.gene.defensiveProperties, partner.gene.defensiveProperties, r);
            Recombinate(child.gene.supportProperties, partner.gene.supportProperties, r);
            Recombinate(child.gene.generalProperties, partner.gene.generalProperties, r);
            Recombinate(child.geneEndTime.attackProperties, partner.gene.attackProperties, r);
            Recombinate(child.geneEndTime.defensiveProperties, partner.gene.defensiveProperties, r);
            Recombinate(child.geneEndTime.supportProperties, partner.gene.supportProperties, r);
            Recombinate(child.geneEndTime.generalProperties, partner.gene.generalProperties, r);
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
            Dictionary<string, int> deffProps = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(new List<int> { 2077, 63, 953, 66, 10 });
            Dictionary<string, int> offProps = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(new List<int> { 3647, 6, 1704, 85, 71 });
            Dictionary<string, int> supProps = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(new List<int> { 1189, 72, 1908, 45, 53 });
            Dictionary<string, int> genProps = Genotype_Advanced.CreateProperties<PropertyNames_General>(new List<int> { 34, 3, 68, 646 });
            Dictionary<string, int> deffProps1 = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(new List<int> { 1528, 51, 1154, 71, 12 });
            Dictionary<string, int> offProps1 = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(new List<int> { 3305, 12, 1921, 65, 74 });
            Dictionary<string, int> supProps1 = Genotype_Advanced.CreateProperties<PropertyNames_Advanced>(new List<int> { 1569, 85, 3582, 65, 84 });
            Dictionary<string, int> genProps1 = Genotype_Advanced.CreateProperties<PropertyNames_General>(new List<int> { 30, 61, 42, 625 });
            gene = new Genotype_Advanced(supProps, offProps, deffProps, genProps);
            geneEndTime = new Genotype_Advanced(supProps1, offProps1, deffProps1, genProps1);
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
            geneEndTime = new Genotype_Advanced(
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
        public Individual_Time_Advanced CopyIndividual() {
            List<int> supGene = new List<int>(gene.supportProperties.Values);
            List<int> offGene = new List<int>(gene.attackProperties.Values);
            List<int> deffGene = new List<int>(gene.defensiveProperties.Values);
            List<int> generalGene = new List<int>(gene.generalProperties.Values);
            List<int> supGene2 = new List<int>(geneEndTime.supportProperties.Values);
            List<int> offGene2 = new List<int>(geneEndTime.attackProperties.Values);
            List<int> deffGene2 = new List<int>(geneEndTime.defensiveProperties.Values);
            List<int> generalGene2 = new List<int>(geneEndTime.generalProperties.Values);
            Individual_Time_Advanced newIndividual = new Individual_Time_Advanced(supGene, offGene, deffGene, generalGene, supGene2, offGene2, deffGene2, generalGene2, 0);
            return newIndividual;
        }
    }
}
