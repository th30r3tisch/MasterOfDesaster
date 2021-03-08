using Game_Server.EA.Models.Advanced;
using Game_Server.KI;
using Game_Server.writer.EA_2;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game_Server.EA {
    class EA_2_Algo : EA_Base<Individual_Advanced, KI_2> {
        public delegate double GaussDelegate(double deviation);

        public EA_2_Algo() : base() {
            _writer = new EA_2_Writer("EA2");
            Evolve(CreatePopulation(), 0);
        }

        private Individual_Advanced TournamentSelection(List<Individual_Advanced> population) {

            List<Individual_Advanced> parents = new List<Individual_Advanced>();
            int populationSize = population.Count;
            while (parents.Count < 2) {
                Individual_Advanced contestantOne = population[_r.Next(0, populationSize)];
                Individual_Advanced contestantTwo = population[_r.Next(0, populationSize)];
                if (contestantOne.fitness > contestantTwo.fitness) {
                    parents.Add(contestantOne);
                }
                else {
                    parents.Add(contestantTwo);
                }
            }
            if (_r.NextDouble() > _recombinationProbability) {
                return parents[0].CopyIndividual();
            }
            else {
                return parents[0].PrepareRecombination(parents[1], _r);
            }
        }

        protected override List<Individual_Advanced> CreateOffspring(List<Individual_Advanced> population) {

            CalculateDominance(population);
            _writer.WriteStats(population);

            List<Individual_Advanced> newPopulation = new List<Individual_Advanced>();
            GaussDelegate gauss = new GaussDelegate(Gauss);
            Individual_Advanced child;
            newPopulation.Add(GetElite(population).CopyIndividual());
            newPopulation[0].number = 0;

            for (int i = 1; i < population.Count; i++) {
                child = TournamentSelection(population);
                child.PrepareMutate(_r, gauss);
                child.number = i;
                newPopulation.Add(child);
            }

            ResetNewPopulation(newPopulation);
            return newPopulation;
        }

        /// <summary>
        /// resets all important values preparing the start of the new generation
        /// </summary>
        /// <param name="newPopulation">new generation</param>
        private void ResetNewPopulation(List<Individual_Advanced> newPopulation) {
            for (int i = 0; i < newPopulation.Count; i++) {
                Individual_Advanced individual = newPopulation[i];
                individual.number = i;
                individual.timestamp.Clear();
                individual.atkScore = 0;
                individual.deffScore = 0;
                individual.suppScore = 0;
                individual.townLifeDeviation = 0;
                individual.name = null;
                individual.dominanceLevel = 0;
            }
        }

        /// <summary>
        /// calculates a random number based on gauss
        /// </summary>
        /// <param name="deviation">standard deviation</param>
        /// <returns>random number based on gauss distribution</returns>
        public static double Gauss(double deviation) {
            double mean = 0;
            Normal normalDist = new Normal(mean, deviation);
            double gaussNum = Math.Round(normalDist.Sample(), 1);
            return gaussNum;
        }

        private void CalculateDominance(List<Individual_Advanced> population) {
            List<Individual_Advanced> iterationList = new List<Individual_Advanced>(population);
            int level = 1;
            foreach (var i1 in iterationList) {
                foreach (var i2 in iterationList) {
                    if (i1.deffScore > i2.deffScore && i1.atkScore > i2.atkScore && i1.suppScore > i2.suppScore) {
                        i1.dominates.Add(i2);
                    }
                    else if (i1.deffScore < i2.deffScore && i1.atkScore < i2.atkScore && i1.suppScore < i2.suppScore) {
                        i1.dominatedByIndividuals += 1;
                    }
                }
                if (i1.dominatedByIndividuals == 0) {
                    i1.dominanceLevel = level;
                }
            }
            while (iterationList.Count > 0) {
                level++;
                for (int i = iterationList.Count; i > 0; i--) {
                    Individual_Advanced individual = iterationList[i - 1];
                    if (individual.dominanceLevel == level - 1) {
                        for (int j = individual.dominates.Count; j >= 0; j--) {
                            if (individual.dominates.Count == 0) {
                                iterationList.Remove(individual);
                            }
                            else {
                                Individual_Advanced dominatedI = individual.dominates[j - 1];
                                dominatedI.dominatedByIndividuals -= 1;
                                if (dominatedI.dominatedByIndividuals == 0) {
                                    dominatedI.dominanceLevel = level;
                                }
                                individual.dominates.Remove(dominatedI);
                            }
                        }
                    }
                }
            }
        }

        private void CalculateCrowdedComparison(List<Individual_Advanced> population) {
            List<Individual_Advanced> iterationList = new List<Individual_Advanced>(population);
            int level = 1;
            while (iterationList.Count > 0) {
                List<Individual_Advanced> dominanceList = new List<Individual_Advanced>();
                for (int i = iterationList.Count; i > 0; i--) {
                    if (iterationList[i].dominanceLevel == level) {
                        dominanceList.Add(iterationList[i]);
                        iterationList.Remove(iterationList[i]);
                    }
                }
                CalculateCrowdedComparisonDeff(dominanceList.OrderBy(o => o.deffScore).ToList());
                CalculateCrowdedComparisonAtk(dominanceList.OrderBy(o => o.atkScore).ToList());
                CalculateCrowdedComparisonOfSupp(dominanceList.OrderBy(o => o.suppScore).ToList());
                level++;
            }
        }

        private void CalculateCrowdedComparisonDeff(List<Individual_Advanced> sortedList) {
            int minValue = sortedList[0].deffScore;
            int maxValue = sortedList[sortedList.Count].deffScore;
            sortedList[0].crowdingDistance = int.MaxValue;
            sortedList[sortedList.Count].crowdingDistance = int.MaxValue;
            for (int i = 1; i < sortedList.Count - 1; i++) {
                sortedList[i].crowdingDistance += (sortedList[i + 1].deffScore - sortedList[i - 1].deffScore) / (maxValue - minValue);
            }
        }
        private void CalculateCrowdedComparisonAtk(List<Individual_Advanced> sortedList) {
            int minValue = sortedList[0].atkScore;
            int maxValue = sortedList[sortedList.Count].atkScore;
            sortedList[0].crowdingDistance = int.MaxValue;
            sortedList[sortedList.Count].crowdingDistance = int.MaxValue;
            for (int i = 1; i < sortedList.Count - 1; i++) {
                sortedList[i].crowdingDistance += (sortedList[i + 1].atkScore - sortedList[i - 1].atkScore) / (maxValue - minValue);
            }
        }
        private void CalculateCrowdedComparisonOfSupp(List<Individual_Advanced> sortedList) {
            int minValue = sortedList[0].suppScore;
            int maxValue = sortedList[sortedList.Count].suppScore;
            sortedList[0].crowdingDistance = int.MaxValue;
            sortedList[sortedList.Count].crowdingDistance = int.MaxValue;
            for (int i = 1; i < sortedList.Count - 1; i++) {
                sortedList[i].crowdingDistance += (sortedList[i + 1].suppScore - sortedList[i - 1].suppScore) / (maxValue - minValue);
            }
        }
    }
}
