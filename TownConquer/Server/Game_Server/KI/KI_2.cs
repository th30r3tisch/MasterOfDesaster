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

        protected override async Task<Individual_Advanced> PlayAsync(CancellationToken ct) {
            indi.startPos = player.towns[0].position;
            int townCount = 0;

            while (Constants.TOWN_NUMBER * 0.8 > player.towns.Count) { //  && player.towns.Count != 0
                try {
                    await Task.Delay(Constants.KI_TICK_RATE);
                }
                catch (Exception _ex) {
                    Console.WriteLine($"{player.username} error: {_ex}");
                }

                if (townCount <= player.towns.Count) {
                    townCount = player.towns.Count;
                }
                else {
                    townCount = player.towns.Count;
                    indi.score -= 5;
                }
                lock (game.gm.treeLock) {
                    for (int i = player.towns.Count; i > 0; i--) {
                        Town atkTown = player.towns[i - 1];
                        CategorizeTown(atkTown);
                    }
                }
                long timeMem = game.gm.sw.ElapsedMilliseconds;
                if (timeMem > protocollTime) {
                    protocollTime += timeMem;
                    ProtocollStats(timeMem);
                }
                if (ct.IsCancellationRequested) {
                    indi.won = false;
                    return indi;
                }
            }
            indi.won = true;
            ProtocollStats(game.gm.sw.ElapsedMilliseconds);
            return indi;
        }

        private void ProtocollStats(long timePassed) {
            indi.name = player.username;
            indi.timestamp.Add(timePassed);
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
                CheckKITownLifes(town, indi.gene.supportProperties);
                TrySupportTown(town, indi.gene.supportProperties);
            }
            if (friendlyPercent <= atkRatio && allTowns > 1) {        
                if (friendlyPercent <= defRatio) {
                    town.townCategory = TownCategory.deff;
                    CheckKITownLifes(town, indi.gene.defensiveProperties);
                }
                else {
                    town.townCategory = TownCategory.off;
                    CheckKITownLifes(town, indi.gene.attackProperties);
                    TryAttackTown(town, indi.gene.attackProperties);
                }
            }
        }

        protected override void CheckKITownLifes(Town town, Dictionary<string, int> props) {
            town.CalculateLife(game.gm.sw.ElapsedMilliseconds, "add action atk");
            if (town.life <= 0) {
                town.life = 0;
                for (int i = town.outgoingActionsToTowns.Count; i > 0; i--) {
                    RetreatFromTown(town.position, town.outgoingActionsToTowns[i - 1].position);
                }
            }
            lock (game.gm.treeLock) {
                for (int x = town.outgoingActionsToTowns.Count; x > 0; x--) {
                    Town t = town.outgoingActionsToTowns[x - 1];
                    t.CalculateLife(game.gm.sw.ElapsedMilliseconds, "add action atk");
                    if (t.life <= 0) {
                        t.life = 0;
                        ConquerTown(player, t.position);
                        indi.score += 20;
                    }
                    else if (t.life > props["SupportMaxCap"] && t.incomingSupporterTowns.Contains(town)) {
                        RetreatFromTown(town.position, t.position);
                    }
                }
            }
        }

        private void TrySupportTown(Town atkTown, Dictionary<string, int> props) {
            List<Town> ownTowns = player.towns;
            foreach (Town supptown in ownTowns) {
                if (game.gm.CanTownsInteract(supptown, atkTown) && supptown.NeedSupport(props["SupportMinCap"])) {
                    InteractWithTown(atkTown.position, supptown.position);
                }
                if (!atkTown.CanSupport(props["SupportMaxCap"])) {
                    return;
                }
            }
        }

        private void TryAttackTown(Town atkTown, Dictionary<string, int> props) {
            if (atkTown.CanAttack(props["AttackMinLife"])) {
                Town deffTown = GetPossibleAttackTarget(atkTown, props);
                if (deffTown != null) {
                    InteractWithTown(atkTown.position, deffTown.position);
                }
            }
        }

        private Town GetPossibleAttackTarget(Town atkTown, Dictionary<string, int> props) {
            int conquerRadius = props["InitialConquerRadius"];
            QuadTree tree = game.tree;

            while (conquerRadius < props["MaxConquerRadius"] && conquerRadius > 0) {
                List<TreeNode> townsInRange;
                List<Town> enemyTowns = new List<Town>();
                Random r = new Random();
                townsInRange = tree.GetAllContentBetween(
                    (int)(atkTown.position.X - conquerRadius),
                    (int)(atkTown.position.Z - conquerRadius),
                    (int)(atkTown.position.X + conquerRadius),
                    (int)(atkTown.position.Z + conquerRadius));

                for (int i = 0; i < townsInRange.Count; i++) {
                    if (townsInRange[i] is Town deffTown) {
                        if (game.gm.CanTownsInteract(deffTown, atkTown) && deffTown.owner != atkTown.owner) {
                            enemyTowns.Add(deffTown);
                        }
                    }
                }
                if (enemyTowns.Count > 0) {
                    return enemyTowns[r.Next(0, enemyTowns.Count - 1)];
                }
                else conquerRadius += props["RadiusExpansionStep"];
            }
            return null;
        }

        public override void Disconnect() {
            indi.won = player.towns.Count >= Constants.TOWN_NUMBER * 0.9;
            ProtocollStats(game.gm.sw.ElapsedMilliseconds);
        }
    }
}
