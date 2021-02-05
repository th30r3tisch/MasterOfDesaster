using Game_Server.EA.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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

        protected void Evolve(List<Individual_Simple> population, int counter) {
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
        protected abstract ConcurrentBag<Individual_Simple> TrainKis(List<Individual_Simple> population);

        protected abstract List<Individual_Simple> Evaluate(ConcurrentBag<Individual_Simple> results);

        protected abstract List<Individual_Simple> CreateOffspring(List<Individual_Simple> population);

        protected abstract void WriteProtocoll(List<Individual_Simple> results);
    }
}
