using Game_Server.EA.Models;
using Game_Server.KI;
using Game_Server.writer.EA_1;
using MathNet.Numerics.Distributions;
using SharedLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.EA {
    class EA_1_Algo: EA_Base<EA_1_Writer> {
        public delegate double GaussDelegate(double deviation);

        public EA_1_Algo() : base() {
            _writer = new EA_1_Writer("EA");
            Evolve(CreatePopulation(), 0);
        }

        protected override List<Individual> CreateOffspring(List<Individual> population) {
            List<Individual> newPopulation = new List<Individual>();
            GaussDelegate gauss = new GaussDelegate(Gauss);
            Individual child;

            newPopulation.Add(GetElite(population));

            for (int i = 0; i < population.Count - 1; i++) {
                child = TournamentSelection(population).DeepCopy();
                child.Mutate(_r, gauss);
                newPopulation.Add(child);
            }

            ResetNewPopulation(newPopulation);
            return newPopulation;
        }

        protected override List<Individual> Evaluate(ConcurrentBag<Individual> results) {
            List<Individual> individualList = new List<Individual>();
            foreach (Individual individual in results) {
                individual.CalcFitness();
                individualList.Add(individual);
            }
            return individualList;
        }

        protected override void WriteProtocoll(List<Individual> results) {
            EA_1_Stat[] stats = new EA_1_Stat[results.Count];
            foreach (var individual in results) {
                EA_1_Stat stat = new EA_1_Stat(individual.name) {
                    //townDevelopment = individual.townNumberDevelopment,
                    //timeStamps = individual.timestamp,
                    gameTime = individual.timestamp.Last(),
                    startPos = individual.startPos,
                    won = individual.won,
                    fitness = individual.fitness,
                    score = individual.score,
                    townLifeSum = individual.townLifeSum,
                    number = individual.number,
                    properties = individual.gene.properties
                };
                stats[individual.number] = stat;
            }
            _writer.WriteStats(stats);
        }


        private Individual CreateIndividual(int number, int initialConquerRadius, int maxConquerRadius, int radiusExpansionStep, int attackMinLife, int supportRadius, int supportMaxCap, int supportMinCap, int supportTownRatio) {
            Genotype g = new Genotype {
                properties = new Dictionary<string, int>() {
                    { "initialConquerRadius", initialConquerRadius },
                    { "maxConquerRadius", maxConquerRadius },
                    { "radiusExpansionStep", radiusExpansionStep },
                    { "attackMinLife", attackMinLife },
                    { "supportRadius", supportRadius },
                    { "supportMaxCap", supportMaxCap },
                    { "supportMinCap", supportMinCap },
                    { "supportTownRatio", supportTownRatio }
                }
            };
            return new Individual(g, number);
        }

        private void ResetNewPopulation(List<Individual> newPopulation) {
            for (int i = 0; i < newPopulation.Count; i++) {
                Individual individual = newPopulation[i];
                individual.number = i;
                individual.townNumberDevelopment.Clear();
                individual.timestamp.Clear();
                individual.score = 0;
                individual.townLifeSum = 0;
            }
        }

        private Individual TournamentSelection(List<Individual> population) {
            List<Individual> parents = new List<Individual>();
            int populationSize = population.Count;
            while (parents.Count < 2) {
                Individual contestantOne = population[_r.Next(0, populationSize)];
                Individual contestantTwo = population[_r.Next(0, populationSize)];
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

        private Individual GetElite(List<Individual> individualList) {
            double bestFitness = -9999;
            Individual eliteIndividual = null;
            foreach (Individual individual in individualList) {
                if (individual.fitness > bestFitness) {
                    eliteIndividual = individual;
                    bestFitness = individual.fitness;
                }
            }
            eliteIndividual.isElite = true;
            
            return eliteIndividual;
        }

        private List<Individual> CreatePopulation() {
            List<Individual> population = new List<Individual>();
            int populationCount = 0;
            while (populationCount < _populationNumber) {
                population.Add(CreateIndividual(
                    populationCount, 
                    _r.Next(Constants.TOWN_MIN_DISTANCE, Constants.MAP_HEIGHT), 
                    _r.Next(Constants.TOWN_MIN_DISTANCE, Constants.MAP_HEIGHT), 
                    _r.Next(- Constants.MAP_HEIGHT / 5, Constants.MAP_HEIGHT / 5), 
                    _r.Next(5, 100),
                    _r.Next(Constants.TOWN_MIN_DISTANCE, Constants.MAP_HEIGHT),
                    _r.Next(5, 1000),
                    _r.Next(5, 1000),
                    _r.Next(0, 100)));
                populationCount++;
            }
            return population;
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
