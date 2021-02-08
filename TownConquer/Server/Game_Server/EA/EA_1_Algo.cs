﻿using Game_Server.EA.Models.Simple;
using Game_Server.KI;
using Game_Server.writer.EA_1;
using MathNet.Numerics.Distributions;
using SharedLibrary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.EA {
    class EA_1_Algo: EA_Base<Individual_Simple> {
        public delegate double GaussDelegate(double deviation);

        public EA_1_Algo() : base() {
            _writer = new EA_1_Writer("EA");
            Evolve(CreatePopulation(), 0);
        }

        protected override ConcurrentBag<Individual_Simple> TrainKis(List<Individual_Simple> population) {
            ConcurrentBag<Individual_Simple> resultCollection = new ConcurrentBag<Individual_Simple>();
            ConcurrentBag<Individual_Simple> referenceCollection = new ConcurrentBag<Individual_Simple>();

            Task[] tasks = population.Select(async individual => {
                GameManager gm = new GameManager();

                CancellationTokenSource c = new CancellationTokenSource();
                CancellationToken token = c.Token;

                KI_Base<Individual_Simple> referenceKI = new KI_1(gm, 999, "REF" + individual.number, Color.FromArgb(0, 0, 0));
                KI_Base<Individual_Simple> eaKI = new KI_1(gm, individual.number, "EA" + individual.number, Color.FromArgb(255, 255, 255));

                Genotype_Simple gene = new Genotype_Simple(new List<int> { 400, 2000, 100, 10 });
                Individual_Simple referenceIndividual = new Individual_Simple(gene, individual.number);

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

        protected override List<Individual_Simple> CreateOffspring(List<Individual_Simple> population) {
            List<Individual_Simple> newPopulation = new List<Individual_Simple>();
            GaussDelegate gauss = new GaussDelegate(Gauss);
            Individual_Simple child;

            newPopulation.Add(GetElite(population));

            for (int i = 0; i < population.Count - 1; i++) {
                child = (Individual_Simple)TournamentSelection(population).DeepCopy();
                child.Mutate(_r, gauss);
                newPopulation.Add(child);
            }

            ResetNewPopulation(newPopulation);
            return newPopulation;
        }

        protected override List<Individual_Simple> Evaluate(ConcurrentBag<Individual_Simple> results) {
            List<Individual_Simple> individualList = new List<Individual_Simple>();
            foreach (Individual_Simple individual in results) {
                individual.CalcFitness();
                individualList.Add(individual);
            }
            return individualList;
        }

        protected override void WriteProtocoll(List<Individual_Simple> results) {
            _writer.WriteStats(results);
        }

        private void ResetNewPopulation(List<Individual_Simple> newPopulation) {
            for (int i = 0; i < newPopulation.Count; i++) {
                Individual_Simple individual = newPopulation[i];
                individual.number = i;
                individual.townNumberDevelopment.Clear();
                individual.timestamp.Clear();
                individual.score = 0;
                individual.townLifeSum = 0;
            }
        }

        private Individual_Simple TournamentSelection(List<Individual_Simple> population) {
            List<Individual_Simple> parents = new List<Individual_Simple>();
            int populationSize = population.Count;
            while (parents.Count < 2) {
                Individual_Simple contestantOne = population[_r.Next(0, populationSize)];
                Individual_Simple contestantTwo = population[_r.Next(0, populationSize)];
                if (contestantOne.fitness > contestantTwo.fitness) {
                    parents.Add(contestantOne);
                }
                else {
                    parents.Add(contestantTwo);
                }
            }
            if (_r.NextDouble() > _recombinationProbability) {
                return parents[0];
            }
            else {
                return parents[0].Recombinate(parents[1], _r);
            }
        }

        private Individual_Simple GetElite(List<Individual_Simple> individualList) {
            double bestFitness = -9999;
            Individual_Simple eliteIndividual = null;
            foreach (Individual_Simple individual in individualList) {
                if (individual.fitness > bestFitness) {
                    eliteIndividual = individual;
                    bestFitness = individual.fitness;
                }
            }
            eliteIndividual.isElite = true;
            
            return eliteIndividual;
        }

        private List<Individual_Simple> CreatePopulation() {
            List<Individual_Simple> population = new List<Individual_Simple>();
            int populationCount = 0;
            while (populationCount < _populationNumber) {
                Genotype_Simple gene = new Genotype_Simple(new List<int> {
                    _r.Next(Constants.TOWN_MIN_DISTANCE, Constants.MAP_HEIGHT),
                    _r.Next(Constants.TOWN_MIN_DISTANCE, Constants.MAP_HEIGHT),
                    _r.Next(-Constants.MAP_HEIGHT / 5, Constants.MAP_HEIGHT / 5),
                    _r.Next(5, 100)
                });
                population.Add(new Individual_Simple(gene, populationCount));
                populationCount++;
            }
            return population;
        }

        /// <summary>
        /// calculates a random number based on gauss
        /// </summary>
        /// <param name="deviation">standard deviation</param>
        /// <returns>random number based on gauss distribution</returns>
        public static double Gauss(double deviation) {
            double mean = 0;
            Normal normalDist = new Normal(mean, deviation);
            double gaussNum = Math.Round(normalDist.Sample(), 1);
            return gaussNum;
        }
    }
}
