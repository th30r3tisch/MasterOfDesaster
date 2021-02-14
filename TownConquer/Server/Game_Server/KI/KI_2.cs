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

        public KI_2(GameManager gm, int id, string name, Color color) : base(gm, id, name, color) { }

        protected override async Task<Individual_Advanced> PlayAsync(CancellationToken ct) {
            i.startPos = player.towns[0].position;
            var startTickCount = Environment.TickCount;
            int timePassed = 0;
            int townCount = 0;

            while (Constants.TOWN_NUMBER * 0.9 > player.towns.Count) { //  && player.towns.Count != 0
                try {
                    await Task.Delay(tickLength);
                }
                catch (Exception _ex) {
                    Console.WriteLine($"{player.username} error: {_ex}");
                }

                if (townCount <= player.towns.Count) {
                    townCount = player.towns.Count;
                }
                else {
                    townCount = player.towns.Count;
                    i.score -= 5;
                }
                lock (gm.treeLock) {
                    for (int i = player.towns.Count; i > 0; i--) {
                        Town atkTown = player.towns[i - 1];
                        CategorizeTown(atkTown);
                    }
                }
                int timeSpan = Environment.TickCount - startTickCount;
                if (timeSpan > protocollTime) {
                    startTickCount = Environment.TickCount;
                    timePassed += protocollTime;
                    ProtocollStats(timePassed);
                }
                if (ct.IsCancellationRequested) {
                    i.won = false;
                    return i;
                }
            }
            i.won = true;
            ProtocollStats(timePassed + Environment.TickCount - startTickCount);
            return i;
        }

        private void ProtocollStats(int timePassed) {
            i.name = player.username;
            i.timestamp.Add(timePassed);
        }

        private void CategorizeTown(Town town) {
            int categorizationRadius = i.gene.generalProperties["CategorisationRadius"];
            int friendlyTownNumber = 0;
            int hostileTownNumber = 0;

            List<TreeNode> objects = gm.game.tree.GetAllContentBetween(
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
            float supRatio = i.gene.generalProperties["SupportTownRatio"] / 100f;
            float atkRatio = i.gene.generalProperties["AtkTownRatio"] / 100f;
            float defRatio = i.gene.generalProperties["DeffTownRatio"] / 100f;
            if (friendlyPercent >= supRatio && allTowns > 1) {
                town.townCategory = TownCategory.sup;
                CheckKITownLifes(town, i.gene.supportProperties);
                TrySupportTown(town, i.gene.supportProperties);
            }
            if (friendlyPercent <= atkRatio && allTowns > 1) {        
                if (friendlyPercent <= defRatio) {
                    town.townCategory = TownCategory.deff;
                    CheckKITownLifes(town, i.gene.defensiveProperties);
                }
                else {
                    town.townCategory = TownCategory.off;
                    CheckKITownLifes(town, i.gene.attackProperties);
                    TryAttackTown(town, i.gene.attackProperties);
                }
            }
        }

        protected override void CheckKITownLifes(Town town, Dictionary<string, int> props) {
            town.CalculateLife(DateTime.Now);
            if (town.life <= 0) {
                town.life = 0;
                for (int i = town.outgoingActionsToTowns.Count; i > 0; i--) {
                    RetreatFromTown(town.position, town.outgoingActionsToTowns[i - 1].position, DateTime.Now);
                }
            }
            lock (gm.treeLock) {
                for (int x = town.outgoingActionsToTowns.Count - 1; x == 0; x--) {
                    Town t = town.outgoingActionsToTowns[x];
                    t.CalculateLife(DateTime.Now);
                    if (t.life <= 0) {
                        t.life = 0;
                        ConquerTown(player, t.position, DateTime.Now);
                        i.score += 20;
                        return;
                    }
                    else if (t.life > props["SupportMaxCap"]) {
                        RetreatFromTown(town.position, t.position, DateTime.Now);
                    }
                }
            }
        }

        private void TrySupportTown(Town atkTown, Dictionary<string, int> props) {
            List<Town> ownTowns = player.towns;
            foreach (Town supptown in ownTowns) {
                if (gm.CanTownsInteract(supptown, atkTown) && supptown.NeedSupport(props["SupportMinCap"])) {
                    InteractWithTown(atkTown.position, supptown.position, DateTime.Now);
                }
                if (!atkTown.CanSupport(props["SupportMinCap"])) {
                    return;
                }
            }
        }

        private void TryAttackTown(Town atkTown, Dictionary<string, int> props) {
            if (atkTown.CanAttack(props["AttackMinLife"])) {
                Town deffTown = GetPossibleAttackTarget(atkTown, props);
                if (deffTown != null) {
                    InteractWithTown(atkTown.position, deffTown.position, DateTime.Now);
                }
            }
        }

        private Town GetPossibleAttackTarget(Town atkTown, Dictionary<string, int> props) {
            int conquerRadius = props["InitialConquerRadius"];
            QuadTree tree = gm.game.tree;

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
                        if (gm.CanTownsInteract(deffTown, atkTown) && deffTown.owner != atkTown.owner) {
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
    }
}
