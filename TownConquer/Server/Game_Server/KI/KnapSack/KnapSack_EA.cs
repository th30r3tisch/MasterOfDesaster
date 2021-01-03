using Game_Server.writer;
using System;
using System.Collections.Generic;

namespace Game_Server.KI.KnapSack {
    class KnapSack_EA {
        List<Item> items = new List<Item>();
        private const int numberOfItems = 30;
        private const int numberOfKnapSacks = 200;
        private const double recombinationProbability = 0.7;
        private const double mutationProbability = 1 / numberOfItems;
        private const int noImprovementLimit = 25;
        Random r;

        KnapsackStat average = new KnapsackStat("average");
        KnapsackStat standardDeviation = new KnapsackStat("standardDeviation");
        KnapsackStat maxValues = new KnapsackStat("maxValues");
        KnapsackStat simpsonDivIndex = new KnapsackStat("SimpsonDivIndex");

        public KnapSack_EA() {
            r = new Random(1);

            GenerateItems();
            Evolve(GenerateInitPopulation(), 0, 0);
            StatsWriter writer = new StatsWriter("KnapSack");
            writer.WriteStats(new[] { average, standardDeviation, maxValues, simpsonDivIndex });
        }

        /// <summary>
        /// Evolves the population
        /// </summary>
        /// <param name="population">the population to evolve</param>
        /// <param name="oldavg">last avg of the population</param>
        /// <param name="noImprovmentCount">counter of how many generations without improvement</param>
        private void Evolve(List<KnapSack> population, int oldavg, int noImprovmentCount) {
            if (noImprovmentCount < noImprovementLimit) {
                Evaluate(population);

                int avg = AverageValueInPopulation(population);
                if (avg <= oldavg) {
                    noImprovmentCount++;
                }
                else {
                    oldavg = avg;
                    noImprovmentCount = 0;
                }

                average.entries.Add(avg);
                standardDeviation.entries.Add(StandardDeviationInPopulation(population, avg));
                maxValues.entries.Add(MaxValueInPopulation(population).first.value);
                simpsonDivIndex.entries.Add(SimpsonsDiversityIndexOfPopulation(population));

                Evolve(CreateOffspring(population), oldavg, noImprovmentCount);
            }
        }

        /// <summary>
        /// Creates a new population of solutions
        /// </summary>
        /// <param name="population">population to create offspring from</param>
        /// <returns>the new population</returns>
        private List<KnapSack> CreateOffspring(List<KnapSack> population) {
            List<KnapSack> newPopulation = new List<KnapSack>();
            List<KnapSack> children;
            (KnapSack, KnapSack) elite = MaxValueInPopulation(population);
            newPopulation.Add(elite.Item1);
            newPopulation.Add(elite.Item2);
            for (int i = 0; i < (numberOfKnapSacks) / 2; i++) {
                children = TournamentSelection(population);
                children = Mutate(children);
                newPopulation.AddRange(children);
            }
            return newPopulation;
        }

        /// <summary>
        /// Mutating solutions
        /// </summary>
        /// <param name="children">solutions to mutate</param>
        /// <returns>mutated solutions</returns>
        private List<KnapSack> Mutate(List<KnapSack> children) {
            foreach (KnapSack child in children) {
                List<int> content = child.content;
                for (int i = 0; i < content.Count; i++) {
                    if (r.NextDouble() < mutationProbability) {
                        content[i] = 1 - content[i];
                    }
                }
            }
            return children;
        }

        /// <summary>
        /// Valuates all knapsack solutions
        /// </summary>
        /// <param name="population">population to evaluate</param>
        private void Evaluate(List<KnapSack> population) {
            for (int i = 0; i < numberOfKnapSacks; i++) {
                KnapSack knapSack = population[i];
                List<int> knapSackContent = knapSack.content;
                knapSack.value = 0;
                // iterates through the binary content of a knapsack
                for (int j = 0; j < numberOfItems; j++) {
                    int item = knapSackContent[j];
                    // look for the coresponding item in the itemlist and add value and weight to the knapsack
                    knapSack.capasity += items[j].weight * item;
                    knapSack.value += items[j].value * item;
                    if (knapSack.capasity > knapSack.maxCapasity) {
                        knapSack.value = 0;
                        j = numberOfItems + 1;
                    }

                }
            }
        }

        /// <summary>
        /// selects two random solutions / knapsacks and compares them.
        /// The solution with the higher value is selected as parent
        /// </summary>
        /// <param name="population">the population to choose from</param>
        /// <returns>the new solutions</returns>
        private List<KnapSack> TournamentSelection(List<KnapSack> population) {
            List<KnapSack> parents = new List<KnapSack>();
            while (parents.Count < 2) {
                KnapSack contestantOne = population[r.Next(0, numberOfKnapSacks)];
                KnapSack contestantTwo = population[r.Next(0, numberOfKnapSacks)];
                if (contestantOne.value > contestantTwo.value) {
                    parents.Add(contestantOne);
                }
                else {
                    parents.Add(contestantTwo);
                }
            }
            if (r.NextDouble() > recombinationProbability) {
                return parents;
            }
            else {
                return Recombinate(parents);
            }
        }

        /// <summary>
        /// Rebombinates two solutions at a random breakpoint
        /// </summary>
        /// <param name="parents">solutions to recombinate</param>
        /// <returns>the new children</returns>
        private List<KnapSack> Recombinate(List<KnapSack> parents) {
            List<KnapSack> children = new List<KnapSack>();
            int splitListAt = r.Next(1, numberOfItems);

            List<int> content1 = parents[0].content.GetRange(0, splitListAt);
            List<int> content2 = parents[1].content.GetRange(0, splitListAt);
            content1.AddRange(parents[1].content.GetRange(splitListAt, numberOfItems - splitListAt));
            content2.AddRange(parents[0].content.GetRange(splitListAt, numberOfItems - splitListAt));

            KnapSack child1 = new KnapSack();
            KnapSack child2 = new KnapSack();
            child1.content = content1;
            child2.content = content2;
            children.Add(child1);
            children.Add(child2);
            return children;
        }

        /// <summary>
        /// Generates an initial population of Knapsacks (Solutions) each filled with a random combination of items
        /// </summary>
        private List<KnapSack> GenerateInitPopulation() {
            List<KnapSack> population = new List<KnapSack>();
            int i = 0;
            while (i < numberOfKnapSacks) {
                KnapSack knapSack = new KnapSack();
                knapSack.CreateRandomContent(numberOfItems, r);
                population.Add(knapSack);
                i++;
            }
            return population;
        }

        /// <summary>
        /// Generates a number of random items which can be stored in the backpack
        /// </summary>
        private void GenerateItems() {
            int i = 0;
            while (i < numberOfItems) {
                items.Add(new Item(r));
                i++;
            }
        }

        private int AverageValueInPopulation(List<KnapSack> population) {
            int averageValue = 0;
            foreach (KnapSack knapSack in population) {
                averageValue += knapSack.value;
            }
            averageValue /= population.Count;
            return averageValue;
        }

        private (KnapSack first, KnapSack second) MaxValueInPopulation(List<KnapSack> population) {
            int max1 = 0;
            int max2 = 0;
            KnapSack first = null;
            KnapSack second = null;
            foreach (KnapSack knapSack in population) {
                int value = knapSack.value;
                if (value > max1) {
                    max2 = max1; max1 = value;
                    second = first; first = knapSack;
                }
                else if (value > max2) {
                    max2 = value;
                    second = knapSack;
                }
            }
            return (first, second);
        }

        private double StandardDeviationInPopulation(List<KnapSack> population, int avg) {
            double varianz = 0;
            foreach (KnapSack knapSack in population) {
                varianz += Math.Pow(knapSack.value - avg, 2);
            }
            varianz /= population.Count;
            return Math.Round(Math.Sqrt(varianz));
        }

        private double SimpsonsDiversityIndexOfPopulation(List<KnapSack> population) {
            double sum = 0;
            double zähler = 0;
            foreach (KnapSack knapSack in population) {
                sum += knapSack.value;
                zähler += knapSack.value * (knapSack.value - 1);
            }
            double nenner = sum * (sum - 1);
            double sdi = Math.Round(1d - (zähler / nenner), 2);
            return sdi;
        }
    }
}
