using Game_Server.EA.Models.Advanced;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    class KI_3 : KI_Base<Individual_Time_Advanced> {

        private int _townCountOld;
        private double _townNumberToWin;

        public KI_3(Game game, int id, string name, Color color) : base(game, id, name, color) {
            _townNumberToWin = Constants.TOWN_NUMBER * 0.9;
        }
        
        /// <summary>
        /// includes the play routine of the ki
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>task with individual</returns>
        protected override async Task<Individual_Time_Advanced> PlayAsync(CancellationToken ct) {
            indi.startPos = player.towns[0].position;
            _townCountOld = 0;
            CategorizeTowns();
            GetCategoryDependentTarget(player.towns[0]);

            while (_townNumberToWin > player.towns.Count && player.towns.Count != 0) { //  
                try {
                    await Task.Delay(Constants.KI_TICK_RATE);
                }
                catch (Exception _ex) {
                    Console.WriteLine($"{player.username} error: {_ex}");
                }

                lock (game.gm.treeLock) {
                    for (int i = player.towns.Count; i > 0; i--) {
                        Town atkTown = player.towns[i - 1];
                        CheckLostTowns();
                        DoAction(atkTown);
                    }
                }
                long timeMem = game.gm.sw.ElapsedMilliseconds;
                if (timeMem > protocollTime) {
                    protocollTime += timeMem;
                    ProtocollStats(timeMem);
                }
                if (ct.IsCancellationRequested) {
                    Disconnect();
                    return indi;
                }
            }
            Disconnect();
            return indi;
        }

        private void CheckLostTowns() {
            int townCountNew = player.towns.Count;
            if (_townCountOld <= townCountNew) {
                _townCountOld = townCountNew;
            }
            else {
                indi.deffScore += 10 * (_townCountOld - townCountNew);
                _townCountOld = townCountNew;
                CategorizeTowns();
            }
        }

        private void GetCategoryDependentTarget(Town t) {
            switch (t.townCategory) {
                case TownCategory.sup:
                    GetPossibleInteractionTarget(t, LinearInterpolation(indi.gene.supportProperties["ConquerRadius"], indi.geneEndTime.supportProperties["ConquerRadius"]));
                    break;
                case TownCategory.deff:
                    GetPossibleInteractionTarget(t, LinearInterpolation(indi.gene.defensiveProperties["ConquerRadius"], indi.geneEndTime.defensiveProperties["ConquerRadius"]));
                    break;
                case TownCategory.off:
                    GetPossibleInteractionTarget(t, LinearInterpolation(indi.gene.attackProperties["ConquerRadius"], indi.geneEndTime.attackProperties["ConquerRadius"]));
                    break;
                default:
                    break;
            }
            
        }

        private void DoAction(Town t) {
            switch (t.townCategory) {
                case TownCategory.sup:
                    CheckKITownLifes(t, indi.gene.supportProperties, indi.geneEndTime.supportProperties);
                    TrySupportTown(t, indi.gene.supportProperties, indi.geneEndTime.supportProperties);
                    break;
                case TownCategory.deff:
                    CheckKITownLifes(t, indi.gene.defensiveProperties, indi.geneEndTime.defensiveProperties);
                    break;
                case TownCategory.off:
                    CheckKITownLifes(t, indi.gene.attackProperties, indi.geneEndTime.attackProperties);
                    TryAttackTown(t, indi.gene.attackProperties, indi.geneEndTime.attackProperties);
                    break;
                default:
                    break;
            }
        }

        private void ProtocollStats(long timePassed) {
            indi.name = player.username;
            indi.timestamp.Add(timePassed);
        }

        private void CategorizeTowns() {
            lock (game.gm.treeLock) {
                foreach (Town town in player.towns) {
                    CategorizeTown(town);
                }
            }
        }

        private void CategorizeTown(Town town) {
            int categorizationRadius = LinearInterpolation(indi.gene.generalProperties["CategorisationRadius"], indi.geneEndTime.generalProperties["CategorisationRadius"]);
            int friendlyTownNumber = 0;
            int hostileTownNumber = 0;

            List<TreeNode> objects = game.tree.GetAllContentBetween(
                (int)town.position.X - categorizationRadius,
                (int)town.position.Z - categorizationRadius,
                (int)town.position.X + categorizationRadius,
                (int)town.position.Z + categorizationRadius);
            foreach (TreeNode node in objects) {
                if (node is Town t) {
                    if (t.owner == player) {
                        friendlyTownNumber++;
                    }
                    else {
                        hostileTownNumber++;
                    }
                }
            }
            float allTowns = friendlyTownNumber + hostileTownNumber;
            float friendlyPercent = friendlyTownNumber / allTowns;
            float supRatio = LinearInterpolation(indi.gene.generalProperties["SupportTownRatio"], indi.geneEndTime.generalProperties["SupportTownRatio"]) / 100f;
            float atkRatio = LinearInterpolation(indi.gene.generalProperties["AtkTownRatio"], indi.geneEndTime.generalProperties["AtkTownRatio"]) / 100f;
            float defRatio = LinearInterpolation(indi.gene.generalProperties["DeffTownRatio"], indi.geneEndTime.generalProperties["DeffTownRatio"]) / 100f;
            if (friendlyPercent >= supRatio && allTowns > 1) {
                town.townCategory = TownCategory.sup;
            }
            if (friendlyPercent <= atkRatio && allTowns > 1) {
                if (friendlyPercent <= defRatio) {
                    town.townCategory = TownCategory.deff;
                }
                else {
                    town.townCategory = TownCategory.off;
                }
            }
        }

        protected void CheckKITownLifes(Town town, Dictionary<string, int> props1, Dictionary<string, int> props2) {
            town.CalculateLife(game.gm.sw.ElapsedMilliseconds, "own life");
            if (town.life <= 0) {
                for (int i = town.outgoingActionsToTowns.Count; i > 0; i--) {
                    RetreatFromTown(town.position, town.outgoingActionsToTowns[i - 1].position);
                }
                town.life = 0;
            }
            for (int x = town.outgoingActionsToTowns.Count; x > 0; x--) {
                Town t = town.outgoingActionsToTowns[x - 1];
                t.CalculateLife(game.gm.sw.ElapsedMilliseconds, "life of outgoing");
                if (t.life <= 0) {
                    ConquerTown(t.position);
                    indi.atkScore += 20;
                    CategorizeTowns();
                    GetCategoryDependentTarget(t);
                }
                else if (t.life > LinearInterpolation(props1["SupportMaxCap"], props2["SupportMaxCap"]) && t.incomingSupporterTowns.Contains(town)) {
                    RetreatFromTown(town.position, t.position);
                }
            }
        }

        private void TrySupportTown(Town sourceTown, Dictionary<string, int> props1, Dictionary<string, int> props2) {
            if (sourceTown.townsInRange.Count <= 0) return;
            List<Town> sortedTowns =  sourceTown.townsInRange.OrderBy(o => o.life).ToList();
            foreach (Town town in sortedTowns) {
                if (!sourceTown.CanSupport(LinearInterpolation(props1["SupportMaxCap"], props2["SupportMaxCap"]))) {
                    return;
                }
                if (town.owner.username.Equals(sourceTown.owner.username) &&
                    town.NeedSupport(LinearInterpolation(props1["SupportMinCap"], props2["SupportMinCap"]))) {
                    InteractWithTown(sourceTown.position, town.position);
                    indi.supportActions++;
                }
                else {
                    TryAttackTown(sourceTown, props1, props2);
                }
            }
        }

        private void TryAttackTown(Town sourceTown, Dictionary<string, int> props1, Dictionary<string, int> props2) {
            if (sourceTown.townsInRange.Count <= 0) return;
            foreach (Town town in sourceTown.townsInRange) {
                if (!sourceTown.CanAttack(LinearInterpolation(props1["AttackMinLife"], props2["AttackMinLife"]))) {
                    return;
                }
                if (!town.owner.username.Equals(sourceTown.owner.username)) {
                    InteractWithTown(sourceTown.position, town.position);
                    indi.attackActions++;
                }
            }
        }

        private void CalcTownLifeDeviation() {
            List<Town> townlist = player.towns;
            double life = 0;
            double varianz = 0;
            if (townlist.Count <= 1) {
                indi.townLifeDeviation = 50;
                return;
            }
            for (int x = townlist.Count; x > 0; x--) {
                life += townlist[x - 1].life;
            }
            double meanLife = life / townlist.Count;
            for (int x = townlist.Count; x > 0; x--) {
                varianz += Math.Pow((townlist[x - 1].life - meanLife), 2);
            }
            indi.townLifeDeviation =  Math.Round(Math.Sqrt(varianz / townlist.Count), 2);
        }

        public override void Disconnect() {
            if (game.kis[0] != this) {
                indi.won = player.towns.Count > game.kis[0].player.towns.Count;
            }
            else {
                indi.won = player.towns.Count > game.kis[1].player.towns.Count;
            }
            CalcTownLifeDeviation();
            ProtocollStats(game.gm.sw.ElapsedMilliseconds);
        }

        private int LinearInterpolation(int startValue, int endValue) {
            return (int)(startValue + (endValue - startValue) / (Constants.TOWN_NUMBER / (player.towns.Count + 0.1)));
        }
    }
}
