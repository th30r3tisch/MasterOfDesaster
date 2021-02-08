using Game_Server.EA.Models;
using Game_Server.writer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Game_Server.EA {
    abstract class EA_Base<T> where T: IIndividual {

        protected const int _populationNumber = 200;
        protected const int _noImprovementLimit = 100;
        protected const double _recombinationProbability = 0.7;

        protected readonly Random _r;
        protected StatsWriter<T> _writer;

        public EA_Base() {
            _r = new Random();
        }

        protected void Evolve(List<T> population, int counter) {
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
        protected abstract ConcurrentBag<T> TrainKis(List<T> population);

        protected abstract List<T> Evaluate(ConcurrentBag<T> results);

        protected abstract List<T> CreateOffspring(List<T> population);

        protected abstract void WriteProtocoll(List<T> results);
    }
}
