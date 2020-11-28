using System;
using System.Collections.Generic;
using System.IO;

namespace Game_Server.KI.KnapSack {
    class KnapSack_EA {
        List<Item> items = new List<Item>();
        int numberOfItems = 10;
        int numberOfKnapSacks = 100;
        double recombinationProbability = 0.7;
        int maxGenerations = 50;
        Random r;

        public KnapSack_EA() {
            r = new Random(1);
            GenerateItems();
            string fileName = @"stats.txt";
            fileName = Path.GetFullPath(fileName);
            using (var w = new StreamWriter(fileName)) {
                Evolve(GenerateInitPopulation(), 0, w);
            }
        }

        /// <summary>
        /// Evolves the population
        /// </summary>
        /// <param name="population">the population to evolve</param>
        /// <param name="generation"> number of generations past</param>
        /// <param name="w">Streamwriter to log statistics</param>
        private void Evolve(List<KnapSack> population, int generation, StreamWriter w) {
            if (generation < maxGenerations) {
                Evaluate(population);
                generation += 1;
                var line = string.Format("{0},", AverageValueInPopulation(population));
                w.Write(line);
                w.Flush();
                Evolve(CreateOffspring(population), generation, w);
            }
        }

        /// <summary>
        /// Creates a new population of solutions
        /// </summary>
        /// <param name="population">population to create offspring from</param>
        /// <returns>the new population</returns>
        private List<KnapSack> CreateOffspring(List<KnapSack> population) {
            List<KnapSack> newPopulation = new List<KnapSack>();
            for (int i = 0; i < numberOfKnapSacks/2; i++) {
                newPopulation.AddRange(TournamentSelection(population));
            }
            return newPopulation;
        }

        /// <summary>
        /// Valuates all knapsack solutions
        /// </summary>
        /// <param name="population">population to evaluate</param>
        private void Evaluate(List<KnapSack> population) {
            for (int i = 0; i < numberOfKnapSacks; i++) {
                KnapSack knapSack = population[i];
                List<int> knapSackContent = knapSack.content;
                // iterates through the binary content of a knapsack
                for (int j = 0; j < numberOfItems; j++) {
                    int item = knapSackContent[j];
                    // if the item in the knapsack is present -> value is 1
                    if (item == 1) {
                        // look for the coresponding item in the itemlist and add value and weight to the knapsack
                        knapSack.capasity += items[j].weight;
                        knapSack.value += items[j].value;
                        if (knapSack.capasity > knapSack.maxCapasity) {
                            knapSack.value = 0;
                            j = numberOfItems + 1;
                        }
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
            List<int> t1 = parents[1].content.GetRange(splitListAt, numberOfItems - splitListAt);
            List<int> t2 = parents[0].content.GetRange(splitListAt, numberOfItems - splitListAt);
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
            Console.WriteLine(averageValue);
            return averageValue;
        }

        private void CheckPrint(List<KnapSack> population) {
            foreach (KnapSack item in population) {
                Console.WriteLine(item.value + " - " + item.capasity);
                //Console.WriteLine("[{0}]", string.Join(", ", item.content));
            }
        }
    }
}
