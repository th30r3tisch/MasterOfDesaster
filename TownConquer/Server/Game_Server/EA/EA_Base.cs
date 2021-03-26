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
        protected const int _noImprovementLimit = 70;
        protected const double _recombinationProbability = 0.5;

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
                population = Evaluate(TrainKis(population).Result);
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
                //population.Add((T)Activator.CreateInstance(typeof(T), new object[] { _r, populationCount }));
                population.Add((T)Activator.CreateInstance(typeof(T), new object[] { populationCount }));
                populationCount++;
            }
            return population;
        }

        /// <summary>
        /// starts the games to evaluate the population of individuals with limited number of tasks from
        /// https://stackoverflow.com/a/10810730/5859685 and https://stackoverflow.com/a/65533998/5859685
        /// </summary>
        /// <param name="population"></param>
        /// <returns></returns>
        protected async Task<ConcurrentBag<T>> TrainKis(List<T> population) {
            ConcurrentBag<T> resultCollection = new ConcurrentBag<T>();
            ConcurrentBag<T> referenceCollection = new ConcurrentBag<T>();
            List<Task> allTasks = new List<Task>();
            SemaphoreSlim throttler = new SemaphoreSlim(initialCount: 16);

            foreach (var individual in population) {
                await throttler.WaitAsync();

                allTasks.Add(
                    Task.Run(async () => {
                        try {
                            Game game = new Game(individual.number);

                            CancellationTokenSource c = new CancellationTokenSource();
                            CancellationToken token = c.Token;

                            K eaKI = (K)Activator.CreateInstance(typeof(K), new object[] { game, individual.number, "EA" + individual.number, Color.FromArgb(0, 0, 0) });
                            KI_Base<Individual_Advanced> referenceKI = new KI_2(game, 999, "KI" + individual.number, Color.FromArgb(255, 255, 255));
                            //K eaKI = (K)Activator.CreateInstance(typeof(K), new object[] { game, individual.number, "EA" + individual.number, Color.FromArgb(0, 0, 0) });

                            Individual_Advanced referenceIndividual = new Individual_Advanced(999);

                            var t1 = referenceKI.SendIntoGame(token, referenceIndividual);
                            var t2 = eaKI.SendIntoGame(token, individual);

                            await Task.WhenAny(t1, t2);
                            c.Cancel();
                            await Task.WhenAll(t1, t2);
                            resultCollection.Add(await t2);
                        }
                        finally {
                            throttler.Release();
                        }
                    })
                );
            }

            // won't get here until all urls have been put into tasks
            await Task.WhenAll(allTasks);

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
