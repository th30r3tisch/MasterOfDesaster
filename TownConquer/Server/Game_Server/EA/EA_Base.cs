using Game_Server.EA.Models;
using Game_Server.EA.Models.Advanced;
using Game_Server.EA.Models.Simple;
using Game_Server.KI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.EA {
    abstract class EA_Base<T,K> where T: IIndividual where K: KI_Base<T> {

        protected const int _populationNumber = 200;
        protected const int _noImprovementLimit = 100;
        protected const double _recombinationProbability = 0.7;

        protected readonly Random _r;
        protected StatsWriter<T> _writer;

        public EA_Base() {
            _r = new Random();
        }

        /// <summary>
        /// includes the evolutionary cycle to improve the individuals
        /// </summary>
        /// <param name="population">list of individuals</param>
        /// <param name="counter">number counting the generations</param>
        protected void Evolve(List<T> population, int counter) {
            if (counter < _noImprovementLimit) {
                Console.WriteLine($"_________Evo {counter}________");
                population = Evaluate(TrainKis(population));
                _writer.WriteStats(population);
                counter++;
                Evolve(CreateOffspring(population), counter);
            }
            else {
                Console.WriteLine("FINISHED");
            }
        }

        /// <summary>
        /// creates the first random population
        /// </summary>
        /// <returns>random population</returns>
        protected List<T> CreatePopulation() {
            List<T> population = new List<T>();
            int populationCount = 0;
            while (populationCount < _populationNumber) {
                population.Add((T)Activator.CreateInstance(typeof(T), new object[] {_r, populationCount}));
                populationCount++;
            }
            return population;
        }

        /// <summary>
        /// starts the games to evaluate the population of individuals
        /// </summary>
        /// <param name="population">list of individuals</param>
        /// <returns>ConcurrentBag with results of each game and individual</returns>
        protected ConcurrentBag<T> TrainKis(List<T> population) {
            ConcurrentBag<T> resultCollection = new ConcurrentBag<T>();
            ConcurrentBag<T> referenceCollection = new ConcurrentBag<T>();

            Task[] tasks = population.Select(async individual => {
                Game game = new Game(individual.number);

                CancellationTokenSource c = new CancellationTokenSource();
                CancellationToken token = c.Token;

                //KI_Base<Individual_Advanced> referenceKI = new KI_2(gm, 999, "KI999", Color.FromArgb(255, 255, 255));
                // K referenceKI = (K)Activator.CreateInstance(typeof(K), new object[] { gm, 999, "REF" + individual.number, Color.FromArgb(0, 0, 0) });
                K eaKI = (K)Activator.CreateInstance(typeof(K), new object[] { game, individual.number, "EA" + individual.number, Color.FromArgb(0, 0, 0) });
                KI_Base<Individual_Simple> referenceKI = new KI_1(game, 999, "KI999", Color.FromArgb(255, 255, 255));
                
                Individual_Simple referenceIndividual = new Individual_Simple(999);
                //Individual_Advanced referenceIndividual = new Individual_Advanced(999);
                //T referenceIndividual = (T)Activator.CreateInstance(typeof(T), new object[] { individual.number });

                var t1 = referenceKI.SendIntoGame(token, referenceIndividual);
                var t2 = eaKI.SendIntoGame(token, individual);

                await Task.WhenAny(t1, t2);
                c.Cancel();
                await Task.WhenAll(t1, t2);
                //var result1 = await t1;
                var result2 = await t2;
                //referenceCollection.Add(result1);
                resultCollection.Add(result2);
            }).ToArray();

            Task.WaitAll(tasks);

            return resultCollection;
        }

        /// <summary>
        /// Searchs the best individual of the population based on fitness
        /// </summary>
        /// <param name="individualList">population</param>
        /// <returns>the best individual</returns>
        protected T GetElite(List<T> individualList) {
            double bestFitness = -9999;
            T eliteIndividual = default;
            foreach (T individual in individualList) {
                if (individual.fitness > bestFitness) {
                    eliteIndividual = individual;
                    bestFitness = individual.fitness;
                }
            }
            eliteIndividual.isElite = true;

            return eliteIndividual;
        }

        /// <summary>
        /// calculating the fitness of each individual
        /// </summary>
        /// <param name="results">all individuals</param>
        /// <returns>list of evaluated individuals</returns>
        protected List<T> Evaluate(ConcurrentBag<T> results) {
            List<T> individualList = new List<T>();
            foreach (T individual in results) {
                individual.CalcFitness();
                individualList.Add(individual);
            }
            return individualList;
        }

        protected abstract List<T> CreateOffspring(List<T> population);
    }
}
