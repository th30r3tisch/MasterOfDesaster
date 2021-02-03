using Game_Server.EA.Models;
using Game_Server.KI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.EA {
    abstract class EA_Base<T> {

        protected const int _populationNumber = 200;
        protected const int _noImprovementLimit = 100;
        protected const double _recombinationProbability = 0.7;

        protected readonly Random _r;
        protected T _writer;

        public EA_Base() {
            _r = new Random();
        }

        protected void Evolve(List<Individual> population, int counter) {
            if (counter < _noImprovementLimit) {
                Console.WriteLine($"_________Evo {counter}________");
                Individual referenceIndividual = CreateIndividual(individual.number, 400, 2000, 100, 10, 500, 600, 50, 85);
                population = Evaluate(TrainKis(population, referenceIndividual));
                WriteProtocoll(population);
                counter++;
                Evolve(CreateOffspring(population), counter);
            }
            else {
                Console.WriteLine("FINISHED");
            }
        }

        protected ConcurrentBag<Individual> TrainKis(List<Individual> population, Individual referenceIndividual) {
            ConcurrentBag<Individual> resultCollection = new ConcurrentBag<Individual>();
            ConcurrentBag<Individual> referenceCollection = new ConcurrentBag<Individual>();

            Task[] tasks = population.Select(async individual => {
                GameManager gm = new GameManager();

                CancellationTokenSource c = new CancellationTokenSource();
                CancellationToken token = c.Token;

                KI_Base referenceKI = new KI_1(gm, 999, "REF" + individual.number, Color.FromArgb(0, 0, 0));
                KI_Base eaKI = new KI_1(gm, individual.number, "EA" + individual.number, Color.FromArgb(255, 255, 255));
                
                var t1 = referenceKI.SendIntoGame(token, referenceIndividual);
                var t2 = eaKI.SendIntoGame(token, individual);

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

        protected abstract List<Individual> Evaluate(ConcurrentBag<Individual> results);

        protected abstract List<Individual> CreateOffspring(List<Individual> population);

        protected abstract void WriteProtocoll(List<Individual> results);
    }
}
