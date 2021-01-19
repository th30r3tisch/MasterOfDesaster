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
        private const int _populationNumber = 100;
        private const int _noImprovementLimit = 2;
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
                Console.WriteLine($"______________Evo {counter}________");
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

                KI_base eaKI = new KI_1(gm, individual.number, "EA" + individual.number, Color.FromArgb(255, 255, 255));
                KI_base referenceKI = new KI_1(gm, 999, "REF" + individual.number, Color.FromArgb(0, 0, 0));
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
            Individual child;

            for (int i = 0; i < population.Count; i++) {
                child = TournamentSelection(population).DeepCopy();
                child = Mutate(child);
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

        private Individual Mutate(Individual child) {
            var childProps = child.gene.properties;
            double mutationProbability = 1 / childProps.Count();
            foreach (string key in childProps.Keys.ToList()) {
                if (_r.NextDouble() < mutationProbability) {
                    //add or substract a small amount to the value (gauss)
                    childProps[key] = Math.Abs(childProps[key] + Gauss(0.5, 10));
                }
            }
            return child;
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
                return Recombinate(parents);
            }
        }

        private Individual Recombinate(List<Individual> parents) {
            double u = RandomDouble();
            var parentOneProps = parents[0].gene.properties;
            var parentTwoProps = parents[1].gene.properties;

            foreach (string key in parentOneProps.Keys.ToList()) {
                // Kind.Ai = u · Elter1.Ai + (1 - u) · Elter2.Ai
                parentOneProps[key] = (int)(u * parentOneProps[key] + (1 - u) * parentTwoProps[key]);
            }

            return parents[0];
        }

        private List<Individual> Evaluate(ConcurrentBag<Individual> results) {
            List<Individual> individualList = new List<Individual>();
            foreach (Individual individual in results) {
                individual.CalcFitness();
                individualList.Add(individual);
            }
            return individualList;
        }

        private List<Individual> CreatePopulation() {
            List<Individual> population = new List<Individual>();
            int populationCount = 0;
            while (populationCount < _populationNumber) {
                population.Add(CreateIndividual(
                    populationCount, 
                    _r.Next(100, Constants.MAP_HEIGHT), 
                    _r.Next(100, Constants.MAP_HEIGHT), 
                    _r.Next(100, Constants.MAP_HEIGHT / 5), 
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
            List<int> test = new List<int>();
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
                    number = individual.number
                };
                if (test.Contains(individual.number)) {
                    Console.WriteLine("t");
                }
                else {
                    test.Add(individual.number);
                }
                stats[individual.number] = stat;
            }
            _writer.WriteStats(stats);
        }

        /// <summary>
        /// calculates a random number based on gauss
        /// </summary>
        /// <param name="deviation">standard deviation</param>
        /// <param name="mutationSize">size of the mutation. preferably powers of ten</param>
        /// <returns>random number based on gauss distribution</returns>
        private int Gauss(double deviation, int mutationSize) {
            double mean = 0;
            Normal normalDist = new Normal(mean, deviation);
            int gaussNum = (int)Math.Round(normalDist.Sample(), 1) * mutationSize;
            return gaussNum;
        }

        /// <summary>
        /// calculates a random double between 0 and 2
        /// </summary>
        /// <returns>random double between 0 and 2</returns>
        private double RandomDouble() {
            return _r.NextDouble() + _r.NextDouble();
        }
    }
}
