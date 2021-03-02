using Game_Server.EA.Models.Simple;
using Game_Server.KI;
using Game_Server.writer.EA_1;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;

namespace Game_Server.EA {
    class EA_1_Algo : EA_Base<Individual_Simple, KI_1> {
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
                return parents[0].CopyIndividual();
            }
            else {
                return parents[1].Recombinate(parents[0], _r);
            }
        }


        protected override List<Individual_Simple> CreateOffspring(List<Individual_Simple> population) {
            List<Individual_Simple> newPopulation = new List<Individual_Simple>();
            Individual_Simple child;
            GaussDelegate gauss = new GaussDelegate(Gauss);
            newPopulation.Add(GetElite(population).CopyIndividual());
            newPopulation[0].number = 0;
            for (int i = 1; i < population.Count; i++) {
                child = TournamentSelection(population);
                child.Mutate(_r, gauss);
                child.number = i;
                newPopulation.Add(child);
            }
            return newPopulation;
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
