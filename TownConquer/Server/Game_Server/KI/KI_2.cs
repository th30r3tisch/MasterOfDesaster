using Game_Server.EA.Models.Advanced;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    class KI_2 : KI_Base<Individual_Advanced> {

        int townCountOld;

        public KI_2(Game game, int id, string name, Color color) : base(game, id, name, color) { }
        
        /// <summary>
        /// includes the play routine of the ki
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>task with individual</returns>
        protected override async Task<Individual_Advanced> PlayAsync(CancellationToken ct) {
            indi.startPos = player.towns[0].position;
            townCountOld = 0;
            CategorizeTowns();
            GetCategoryDependentTarget(player.towns[0]);

            while (Constants.TOWN_NUMBER * 0.9 > player.towns.Count && player.towns.Count != 0) { //  
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

        /// <summary>
        /// checks how many towns are lost
        /// </summary>
        private void CheckLostTowns() {
            int townCountNew = player.towns.Count;
            if (townCountOld <= townCountNew) {
                townCountOld = townCountNew;
            }
            else {
                indi.deffScore += 10 * (townCountOld - townCountNew);
                townCountOld = townCountNew;
                CategorizeTowns();
            }
        }

        /// <summary>
        /// gets the possible targets of a city according to its category
        /// </summary>
        /// <param name="t">city which needs targets</param>
        private void GetCategoryDependentTarget(Town t) {
            switch (t.townCategory) {
                case TownCategory.sup:
                    GetPossibleInteractionTarget(t, indi.gene.supportProperties["ConquerRadius"]);
                    break;
                case TownCategory.deff:
                    GetPossibleInteractionTarget(t, indi.gene.defensiveProperties["ConquerRadius"]);
                    break;
                case TownCategory.off:
                    GetPossibleInteractionTarget(t, indi.gene.attackProperties["ConquerRadius"]);
                    break;
                default:
                    break;
            }
            
        }

        /// <summary>
        /// executes actions according to the category of a town
        /// </summary>
        /// <param name="t">town which should do an action</param>
        private void DoAction(Town t) {
            switch (t.townCategory) {
                case TownCategory.sup:
                    CheckKITownLifes(t, indi.gene.supportProperties);
                    TrySupportTown(t, indi.gene.supportProperties);
                    break;
                case TownCategory.deff:
                    CheckKITownLifes(t, indi.gene.defensiveProperties);
                    break;
                case TownCategory.off:
                    CheckKITownLifes(t, indi.gene.attackProperties);
                    TryAttackTown(t, indi.gene.attackProperties);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// loggs the data during the game
        /// </summary>
        /// <param name="timePassed">time stamp of the log action</param>
        private void ProtocollStats(long timePassed) {
            indi.name = player.username;
            indi.timestamp.Add(timePassed);
        }

        /// <summary>
        /// categorizes all towns
        /// </summary>
        private void CategorizeTowns() {
            lock (game.gm.treeLock) {
                foreach (Town town in player.towns) {
                    CategorizeTown(town);
                }
            }
        }

        /// <summary>
        /// categorizes one town
        /// </summary>
        /// <param name="town">town to categorize</param>
        private void CategorizeTown(Town town) {
            int categorizationRadius = indi.gene.generalProperties["CategorisationRadius"];
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
            float supRatio = indi.gene.generalProperties["SupportTownRatio"] / 100f;
            float atkRatio = indi.gene.generalProperties["AtkTownRatio"] / 100f;
            float defRatio = indi.gene.generalProperties["DeffTownRatio"] / 100f;
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

        /// <summary>
        /// checks if the town or its interacting towns are below 0 life and starts actions according to townlife
        /// </summary>
        /// <param name="town">the town to check for life points</param>
        /// <param name="props">gene properties</param>
        protected void CheckKITownLifes(Town town, Dictionary<string, int> props) {
            town.CalculateLife(game.gm.sw.ElapsedMilliseconds);
            if (town.life <= 0) {
                for (int i = town.outgoingActionsToTowns.Count; i > 0; i--) {
                    RetreatFromTown(town.position, town.outgoingActionsToTowns[i - 1].position);
                }
                town.life = 0;
            }
            for (int x = town.outgoingActionsToTowns.Count; x > 0; x--) {
                Town t = town.outgoingActionsToTowns[x - 1];
                t.CalculateLife(game.gm.sw.ElapsedMilliseconds);
                if (t.life <= 0) {
                    ConquerTown(t.position);
                    indi.atkScore += 20;
                    CategorizeTowns();
                    GetCategoryDependentTarget(t);
                }
                else if (t.life > props["SupportMaxCap"] && t.incomingSupporterTowns.Contains(town)) {
                    RetreatFromTown(town.position, t.position);
                }
            }
        }

        /// <summary>
        /// tries to support a town. if no support possible it tries to attack instead
        /// </summary>
        /// <param name="sourceTown">town that wants to support</param>
        /// <param name="props">gene properties</param>
        private void TrySupportTown(Town sourceTown, Dictionary<string, int> props) {
            if (sourceTown.townsInRange.Count <= 0) return;
            foreach (Town town in sourceTown.townsInRange) {
                if (!sourceTown.CanSupport(props["SupportMaxCap"])) {
                    return;
                }
                if (town.owner.username.Equals(sourceTown.owner.username) &&
                    town.NeedSupport(props["SupportMinCap"])) {
                    InteractWithTown(sourceTown.position, town.position);
                    indi.supportActions++;
                }
                else {
                    TryAttackTown(sourceTown, props);
                }
            }
        }

        /// <summary>
        /// tries to attack a town
        /// </summary>
        /// <param name="sourceTown">town that wants to attack</param>
        /// <param name="props">gene properties</param>
        private void TryAttackTown(Town sourceTown, Dictionary<string, int> props) {
            if (sourceTown.townsInRange.Count <= 0) return;
            foreach (Town town in sourceTown.townsInRange) {
                if (!sourceTown.CanAttack(props["AttackMinLife"])) {
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

        /// <summary>
        /// finalizes the logged data after game is over
        /// </summary>
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
    }
}
