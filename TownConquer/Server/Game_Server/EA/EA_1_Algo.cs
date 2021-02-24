using Game_Server.EA.Models.Simple;
using Game_Server.KI;
using Game_Server.writer.EA_1;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;

namespace Game_Server.EA {
    class EA_1_Algo: EA_Base<Individual_Simple, KI_1> {
        public delegate double GaussDelegate(double deviation);

        public EA_1_Algo() : base() {
            _writer = new EA_1_Writer("EA1");
            Evolve(CreatePopulation(), 0);
        }

        private Individual_Simple TournamentSelection(List<Individual_Simple> population) {
            
            List<Individual_Simple> parents = new List<Individual_Simple>();
            int populationSize = population.Count;
            while (parents.Count < 2) {
                Individual_Simple contestantOne = population[_r.Next(0, populationSize)];
                Individual_Simple contestantTwo = population[_r.Next(0, populationSize)];
                if (contestantOne.fitness > contestantTwo.fitness) {
                    parents.Add(contestantOne);
                }
                else {
                    parents.Add(contestantTwo);
                }
            }
            if (_r.NextDouble() > _recombinationProbability) {
                return parents[0];
            }
            else {
                return parents[0].Recombinate(parents[1], _r);
            }
        }


        protected override List<Individual_Simple> CreateOffspring(List<Individual_Simple> population) {
            List<Individual_Simple> newPopulation = new List<Individual_Simple>();
            GaussDelegate gauss = new GaussDelegate(Gauss);
            Individual_Simple child;

            newPopulation.Add(GetElite(population));

            for (int i = 0; i < population.Count - 1; i++) {
                child = (Individual_Simple)TournamentSelection(population).DeepCopy();
                child.Mutate(_r, gauss);
                newPopulation.Add(child);
            }

            ResetNewPopulation(newPopulation);
            return newPopulation;
        }

        /// <summary>
        /// resets all important values preparing the start of the new generation
        /// </summary>
        /// <param name="newPopulation">new generation</param>
        private void ResetNewPopulation(List<Individual_Simple> newPopulation) {
            for (int i = 0; i < newPopulation.Count; i++) {
                Individual_Simple individual = newPopulation[i];
                individual.number = i;
                individual.townNumberDevelopment.Clear();
                individual.timestamp.Clear();
                individual.score = 0;
                individual.townLifeSum = 0;
                individual.name = null;
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
    }
}
