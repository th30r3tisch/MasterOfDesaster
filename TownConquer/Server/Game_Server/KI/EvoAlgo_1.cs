using Game_Server.KI.Models;
using Game_Server.writer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    class EvoAlgo_1 {

        StatsWriter writer;
        const int populationNumber = 10;
        const int noImprovementLimit = 1;

        public EvoAlgo_1() {
            writer = new StatsWriter("EA");
            Evolve(CreatePopulation(), 0);
        }

        private void Evolve(List<Individual> population, int counter) {
            if (counter < noImprovementLimit) {
                population = Evaluate(TrainKis(population));
                counter++;
                Evolve(population, counter);
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

                KI_base eaKI = new KI_Stupid(gm, individual.number, "KI-" + individual.number, Color.FromArgb(255, 255, 255));
                KI_base referenceKI = new KI_Stupid(gm, 999, "REF-" + individual.number, Color.FromArgb(0, 0, 0));
                Individual referenceIndividual = CreateIndividual(individual.number, "REF", 400, 2000);

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

            return resultCollection;
        }

        private List<Individual> Evaluate(ConcurrentBag<Individual> results) {
            StatEntry[] stats = new StatEntry[results.Count];
            foreach (var individual in results) {
                StatEntry stat = new StatEntry(individual.name + individual.number);
                stat.entries = individual.result.townNumberDevelopment.ConvertAll(x => (double)x);
                stats[individual.number] = stat;
            }
            writer.WriteStats(stats);
            return results.ToList();
        }

        private List<Individual> CreatePopulation() {
            List<Individual> population = new List<Individual>();
            int populationCount = 0;
            while (populationCount < populationNumber) {
                population.Add(CreateIndividual(populationCount, "EA", 400, 2000));
                populationCount++;
            }
            return population;
        }

        private Individual CreateIndividual(int number, string name, int icr, int mcr) {
            Genotype g = new Genotype {
                initialConquerRadius = icr,
                maxConquerRadius = mcr
            };
            return new Individual(number, name, g);
        }

    }
}
