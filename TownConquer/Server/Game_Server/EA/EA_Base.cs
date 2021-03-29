using Game_Server.EA.Models;
using Game_Server.EA.Models.Advanced;
using Game_Server.EA.Models.Simple;
using Game_Server.KI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
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
                population.Add((T)Activator.CreateInstance(typeof(T), new object[] { _r, populationCount }));
                populationCount++;
            }
            return population;
        }

        /// <summary>
        /// BASIERT AUF DEM CODE VON Theodor Zoulias und Theo Yaung SIEHE:
        /// Yaung, Theo, 30 May 2012, https://stackoverflow.com/a/10810730/5859685 [23.03.2021]
        /// Zoulias, Theodor, 1 Jan 2021, https://stackoverflow.com/a/65533998/5859685 [23.03.2021]
        /// Der Code wurde bearbeitet und Anpassungen vorgenommen.
        /// Dieser Code spielt im Rahmen der Arbeit nur eine große Rolle.
        /// 
        /// starts the games to evaluate the population of individuals with limited number of tasks from
        /// </summary>
        /// <param name="population">all individuals of one generation</param>
        /// <returns>individuals filled with data from playing games</returns>
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
                            KI_Base<Individual_Simple> referenceKI = new KI_1(game, 999, "KI" + individual.number, Color.FromArgb(255, 255, 255));
                            //K eaKI = (K)Activator.CreateInstance(typeof(K), new object[] { game, individual.number, "EA" + individual.number, Color.FromArgb(0, 0, 0) });

                            Individual_Simple referenceIndividual = new Individual_Simple(999);

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
