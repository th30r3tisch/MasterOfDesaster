using Game_Server.EA.Models;
using Game_Server.KI;
using Game_Server.writer;
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
    class EA_1_Algo {
        public delegate double GaussDelegate(double deviation);

        private const int _populationNumber = 100;
        private const int _noImprovementLimit = 100;
        private const double _recombinationProbability = 0.7;

        private readonly Random _r;
        private readonly StatsWriter _writer;

        public EA_1_Algo() {
            _r = new Random();
            _writer = new StatsWriter("EA");
            Evolve(CreatePopulation(), 0);
        }

        private void Evolve(List<Individual> population, int counter) {
            if (counter < _noImprovementLimit) {
                Console.WriteLine($"_________Evo {counter}________");

                population = Evaluate(TrainKis(population));
                WriteProtocoll(population);
                counter++;
                Evolve(CreateOffspring(population), counter);
            }
            else {
                Console.WriteLine("FINISHED");
            }
        }

        private ConcurrentBag<Individual> TrainKis(List<Individual> population) {
            ConcurrentBag<Individual> resultCollection = new ConcurrentBag<Individual>();
            ConcurrentBag<Individual> referenceCollection = new ConcurrentBag<Individual>();

            Task[] tasks = population.Select(async individual => {
                GameManager gm = new GameManager();

                CancellationTokenSource c = new CancellationTokenSource();
                CancellationToken token = c.Token;

                KI_base referenceKI = new KI_1(gm, 999, "REF" + individual.number, Color.FromArgb(0, 0, 0));
                KI_base eaKI = new KI_1(gm, individual.number, "EA" + individual.number, Color.FromArgb(255, 255, 255));
                Individual referenceIndividual = CreateIndividual(individual.number, 400, 2000, 100, 10);

                
                var t1 = referenceKI.Start(token, referenceIndividual);
                var t2 = eaKI.Start(token, individual);

                await Task.WhenAny(t1, t2);
                c.Cancel();
                await Task.WhenAll(t1, t2);
                var result1 = await t1;
                var result2 = await t2;
                referenceCollection.Add(result1);
                resultCollection.Add(result2);
            }).ToArray();

            Task.WaitAll(tasks);

            //WriteProtocoll(referenceCollection, "REF");

            return resultCollection;
        }

        private List<Individual> CreateOffspring(List<Individual> population) {
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

        private void ResetNewPopulation(List<Individual> newPopulation) {
            for(int i = 0; i < newPopulation.Count; i++) {
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

        private List<Individual> Evaluate(ConcurrentBag<Individual> results) {
            List<Individual> individualList = new List<Individual>();
            foreach (Individual individual in results) {
                individual.CalcFitness();
                individualList.Add(individual);
            }
            return individualList;
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
                    _r.Next(5, 100)));
                populationCount++;
            }
            return population;
        }

        private Individual CreateIndividual(int number, int initialConquerRadius, int maxConquerRadius, int radiusExpansionStep, int attackMinLife) {
            Genotype g = new Genotype {
                properties = new Dictionary<string, int>() {
                    { "initialConquerRadius", initialConquerRadius },
                    { "maxConquerRadius", maxConquerRadius },
                    { "radiusExpansionStep", radiusExpansionStep },
                    { "attackMinLife", attackMinLife }
                }
            };
            return new Individual(g, number);
        }

        private void WriteProtocoll(List<Individual> results) {
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
