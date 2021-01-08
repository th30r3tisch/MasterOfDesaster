using Game_Server.EA.Models;
using Game_Server.KI;
using Game_Server.writer;
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
        private const int _noImprovementLimit = 1;

        public EA_1_Algo() {
            Evolve(CreatePopulation(), 0);
        }

        private void Evolve(List<Individual> population, int counter) {
            if (counter < _noImprovementLimit) {
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

                KI_base eaKI = new KI_1(gm, individual.number, "STATIC" + individual.number, Color.FromArgb(255, 255, 255));
                KI_base referenceKI = new KI_1(gm, 999, "REF" + individual.number, Color.FromArgb(0, 0, 0));
                Individual referenceIndividual = CreateIndividual(individual.number, 400, 2000);

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

            WriteProtocoll(referenceCollection, "REF");
            WriteProtocoll(resultCollection, "STATIC");

            return resultCollection;
        }

        private List<Individual> Evaluate(ConcurrentBag<Individual> results) {
            return results.ToList();
        }

        private List<Individual> CreatePopulation() {
            List<Individual> population = new List<Individual>();
            int populationCount = 0;
            while (populationCount < _populationNumber) {
                population.Add(CreateIndividual(populationCount, 400, 2000));
                populationCount++;
            }
            return population;
        }

        private Individual CreateIndividual(int number, int icr, int mcr) {
            Genotype g = new Genotype {
                initialConquerRadius = icr,
                maxConquerRadius = mcr
            };
            return new Individual(g, number);
        }

        private List<Individual> WriteProtocoll(ConcurrentBag<Individual> results, string filename) {
            StatsWriter writer = new StatsWriter(filename);
            EA_1_Stat[] stats = new EA_1_Stat[results.Count];
            foreach (var individual in results) {
                EA_1_Stat stat = new EA_1_Stat(individual.name) {
                    townDevelopment = individual.result.townNumberDevelopment,
                    timeStamps = individual.result.timestamp,
                    startPos = individual.startPos,
                    won = individual.won
                };
                stats[individual.number] = stat;
            }
            writer.WriteStats(stats);
            return results.ToList();
        }
    }
}
