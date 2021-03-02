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

        public KI_2(Game game, int id, string name, Color color) : base(game, id, name, color) { }

        /// <summary>
        /// includes the play routine of the ki
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>task with individual</returns>
        protected override async Task<Individual_Advanced> PlayAsync(CancellationToken ct) {
            indi.startPos = player.towns[0].position;
            int townCountOld = 0;

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
                    int townCountNew = player.towns.Count;
                    if (townCountOld <= townCountNew) {
                        townCountOld = townCountNew;
                    }
                    else {
                        indi.score -= 5 * (townCountOld - townCountNew);
                        townCountOld = townCountNew;
                        CategorizeTowns();
                    }
                    for (int i = player.towns.Count; i > 0; i--) {
                        Town atkTown = player.towns[i - 1];
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

        protected override void CheckKITownLifes(Town town, Dictionary<string, int> props) {
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
                    ConquerTown(player, t.position);
                    indi.score += 20;
                    CategorizeTowns();
                    GetCategoryDependentTarget(t);
                }
                else if (t.life > props["SupportMaxCap"] && t.incomingSupporterTowns.Contains(town)) {
                    RetreatFromTown(town.position, t.position);
                }
            }

        }

        private void TrySupportTown(Town sourceTown, Dictionary<string, int> props) {
            if (sourceTown.townsInRange.Count <= 0) return;
            foreach (Town town in sourceTown.townsInRange) {
                if (!sourceTown.CanSupport(props["SupportMaxCap"])) {
                    return;
                }
                if (town.owner.username.Equals(sourceTown.owner.username) &&
                    town.NeedSupport(props["SupportMinCap"])) {
                    InteractWithTown(sourceTown.position, town.position);
                }
                else {
                    TryAttackTown(sourceTown, props);
                }
            }
        }

        private void TryAttackTown(Town sourceTown, Dictionary<string, int> props) {
            if (sourceTown.townsInRange.Count <= 0) return;
            foreach (Town town in sourceTown.townsInRange) {
                if (!sourceTown.CanAttack(props["AttackMinLife"])) {
                    return;
                }
                if (!town.owner.username.Equals(sourceTown.owner.username)) {
                    InteractWithTown(sourceTown.position, town.position);
                }
            }
        }

        public override void Disconnect() {
            if (game.kis[0] != this) {
                indi.won = player.towns.Count > game.kis[0].player.towns.Count;
            }
            else {
                indi.won = player.towns.Count > game.kis[1].player.towns.Count;
            }
            ProtocollStats(game.gm.sw.ElapsedMilliseconds);
        }
    }
}
