using Game_Server.EA.Models.Advanced;
using Game_Server.KI;
using Game_Server.writer.EA_2;
using SharedLibrary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.EA {
    class EA_2_Algo: EA_Base<Individual_Advanced> {

        public EA_2_Algo() : base() {
            _writer = new EA_2_Writer("EA");
        }

        protected override List<Individual_Advanced> CreateOffspring(List<Individual_Advanced> population) {
            throw new System.NotImplementedException();
        }

        protected override List<Individual_Advanced> Evaluate(ConcurrentBag<Individual_Advanced> results) {
            throw new System.NotImplementedException();
        }

        protected override ConcurrentBag<Individual_Advanced> TrainKis(List<Individual_Advanced> population) {
            ConcurrentBag<Individual_Advanced> resultCollection = new ConcurrentBag<Individual_Advanced>();
            ConcurrentBag<Individual_Advanced> referenceCollection = new ConcurrentBag<Individual_Advanced>();

            Task[] tasks = population.Select(async individual => {
                GameManager gm = new GameManager();

                CancellationTokenSource c = new CancellationTokenSource();
                CancellationToken token = c.Token;

                KI_Base<Individual_Advanced> referenceKI = new KI_2(gm, 999, "REF" + individual.number, Color.FromArgb(0, 0, 0));
                KI_Base<Individual_Advanced> eaKI = new KI_2(gm, individual.number, "EA" + individual.number, Color.FromArgb(255, 255, 255));

                Dictionary<string, int> deffProps = Genotype_Advanced.CreateProperties(new List<int> { 400, 2000, 100, 10, 1000, 100, 20, 85 });
                Dictionary<string, int> offProps = Genotype_Advanced.CreateProperties(new List<int> { 400, 2000, 100, 10, 1000, 100, 20, 85 });
                Dictionary<string, int> supProps = Genotype_Advanced.CreateProperties(new List<int> { 400, 2000, 100, 10, 1000, 100, 20, 85 });
                Genotype_Advanced gene = new Genotype_Advanced(supProps, offProps, deffProps);
                Individual_Advanced referenceIndividual = new Individual_Advanced(gene, individual.number);

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

        private List<Individual_Advanced> CreatePopulation() {
            List<Individual_Advanced> population = new List<Individual_Advanced>();
            int populationCount = 0;
            while (populationCount < _populationNumber) {
                Genotype_Advanced gene = new Genotype_Advanced(GenerateRndProps(), GenerateRndProps(), GenerateRndProps());
                population.Add(new Individual_Advanced(gene, populationCount));
                populationCount++;
            }
            return population;
        }

        private Dictionary<string, int> GenerateRndProps() {
            return Genotype_Advanced.CreateProperties( new List<int> {
                    _r.Next(Constants.TOWN_MIN_DISTANCE, Constants.MAP_HEIGHT),
                    _r.Next(Constants.TOWN_MIN_DISTANCE, Constants.MAP_HEIGHT),
                    _r.Next(-Constants.MAP_HEIGHT / 5, Constants.MAP_HEIGHT / 5),
                    _r.Next(5, 100),
                    _r.Next(Constants.TOWN_MIN_DISTANCE, Constants.MAP_HEIGHT),
                    _r.Next(5, 1000),
                    _r.Next(5, 1000),
                    _r.Next(0, 100)
                });
        }

        protected override void WriteProtocoll(List<Individual_Advanced> results) {
            throw new System.NotImplementedException();
        }
    }
}
