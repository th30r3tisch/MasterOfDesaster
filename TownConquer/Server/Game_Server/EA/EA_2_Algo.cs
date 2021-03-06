﻿using Game_Server.EA.Models.Advanced;
using Game_Server.KI;
using Game_Server.writer.EA_2;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;

namespace Game_Server.EA {
    class EA_2_Algo: EA_Base<Individual_Advanced, KI_2> {
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
            List<Individual_Advanced> newPopulation = new List<Individual_Advanced>();
            GaussDelegate gauss = new GaussDelegate(Gauss);
            Individual_Advanced child;
            CalculateDominance(population);
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
                individual.dominance = 0;
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
            while (iterationList.Count > 0) {
                foreach (var individualOne in iterationList) {
                    individualOne.dominates = false;
                    foreach (var individualTwo in iterationList) {
                        if (individualOne.deffScore > individualTwo.deffScore && individualOne.atkScore > individualTwo.atkScore && individualOne.suppScore > individualTwo.suppScore) {
                            individualOne.dominates = true;
                            break;
                        }
                    }
                }
                for (int k = iterationList.Count; k > 0; k--) {
                    Individual_Advanced individual = iterationList[k - 1];
                    if (!individual.dominates) {
                        individual.dominance = level;
                        iterationList.Remove(individual);
                    }
                }
                level++;
            } 
        }
    }
}
