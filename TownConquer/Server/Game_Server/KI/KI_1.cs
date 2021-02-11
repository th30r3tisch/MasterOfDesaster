using Game_Server.EA.Models.Simple;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    class KI_1 : KI_Base<Individual_Simple> {

        public KI_1(GameManager gm, int id, string name, Color color) : base(gm, id, name, color) { }

        /// <summary>
        /// includes the play routine of the ki
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>task with individual</returns>
        protected override async Task<Individual_Simple> PlayAsync(CancellationToken ct) {
            i.startPos = player.towns[0].position;
            var startTickCount = Environment.TickCount;
            int timePassed = 0;
            int townCount = 0;

            while (Constants.TOWN_NUMBER * 0.8 > player.towns.Count ) { //  && player.towns.Count != 0
                Console.WriteLine($"{player.towns.Count}");
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
                    for (int x = player.towns.Count; x > 0; x--) {
                        Town atkTown = player.towns[x - 1];
                        CheckKITownLifes(atkTown, i.gene.properties);
                        if (IsSupportTown(atkTown)) {
                            TrySupportTown(atkTown);
                        }
                        else {
                            TryAttackTown(atkTown);
                        }
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
                    CalcTownLifeSum();
                    return i;
                }
            }
            i.won = true;
            CalcTownLifeSum();
            ProtocollStats(timePassed + Environment.TickCount - startTickCount);
            return i;
        }

        protected override void CheckKITownLifes(Town town, Dictionary<string, int> props) {
            town.CalculateLife(DateTime.Now);
            if (town.life <= 0) {
                town.life = 0;
                for (int i = town.outgoing.Count; i > 0; i--) {
                    RetreatFromTown(town.position, town.outgoing[i - 1].position, DateTime.Now);
                }
            }
            lock (gm.treeLock) {
                foreach (Town t in town.outgoing) {
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

        private void TrySupportTown(Town atkTown) {
            List<Town> ownTowns = player.towns;
            foreach (Town supptown in ownTowns) {
                if (supptown.life < i.gene.properties["SupportMinCap"] && 
                    atkTown.outgoing.Count < 2 && 
                    !supptown.outgoing.Contains(atkTown) &&
                    !atkTown.outgoing.Contains(supptown) && 
                    atkTown != supptown) {
                        InteractWithTown(atkTown.position, supptown.position, DateTime.Now);
                }
            }   
        }

        private bool IsSupportTown(Town atkTown) {
            int supportRadius = i.gene.properties["SupportRadius"];
            int friendlyTownNumber = 0;
            int hostileTownNumber = 0;
            bool isSupTown = false;

            if (atkTown.life < i.gene.properties["SupportMinCap"]) {
                return false;
            }

            List<TreeNode> objects = gm.game.tree.GetAllContentBetween(
                (int)atkTown.position.X - supportRadius,
                (int)atkTown.position.Z - supportRadius,
                (int)atkTown.position.X + supportRadius,
                (int)atkTown.position.Z + supportRadius);
            foreach (TreeNode node in objects) {
                if (node is Town t) {
                    if (t.player == player) {
                        friendlyTownNumber++;
                    }
                    else {
                        hostileTownNumber++;
                    }
                }
            }
            float allTowns = friendlyTownNumber + hostileTownNumber;
            float friendlyPercent = friendlyTownNumber / allTowns;
            float test = i.gene.properties["SupportTownRatio"] / 100f;
            if (friendlyPercent >= test && allTowns > 1) {
                isSupTown = true;
            }
            return isSupTown;
        }

        private void CalcTownLifeSum() {
            int life = 0;
            foreach (Town town in player.towns) {
                life += town.life;
            }
            i.townLifeSum = life;
        }

        private void ProtocollStats(int timePassed) {
            i.name = player.username;
            i.timestamp.Add(timePassed);
            i.townNumberDevelopment.Add(player.towns.Count);
        }

        private void TryAttackTown(Town atkTown) {
            if (atkTown.life > i.gene.properties["AttackMinLife"] && atkTown.outgoing.Count < 2) {
                Town deffTown = GetPossibleAttackTarget(atkTown);
                if (deffTown != null) {
                    InteractWithTown(atkTown.position, deffTown.position, DateTime.Now);
                }
            }
        }

        private Town GetPossibleAttackTarget(Town atkTown) {
            int conquerRadius = i.gene.properties["InitialConquerRadius"];
            Town target = null;
            QuadTree tree = gm.game.tree;

            while (target == null && conquerRadius < i.gene.properties["MaxConquerRadius"] && conquerRadius > 0) {
                List <TreeNode> townsInRange;
                List<Town> enemyTowns = new List<Town>();
                Random r = new Random();
                townsInRange = tree.GetAllContentBetween(
                    (int)(atkTown.position.X - conquerRadius),
                    (int)(atkTown.position.Z - conquerRadius),
                    (int)(atkTown.position.X + conquerRadius),
                    (int)(atkTown.position.Z + conquerRadius));

                for (int i = 0; i < townsInRange.Count; i++) {
                    if (townsInRange[i] is Town deffTown) {
                        if (!deffTown.player.username.Equals(atkTown.player.username) &&
                        !gm.IsIntersecting(atkTown.position, deffTown.position) &&
                        !atkTown.outgoing.Contains(deffTown)) {
                            enemyTowns.Add(deffTown);
                        }
                    }
                }
                if (enemyTowns.Count > 0) {
                    return enemyTowns[r.Next(0, enemyTowns.Count - 1)];
                }
                else conquerRadius += i.gene.properties["RadiusExpansionStep"];
            }
            return null;
        }
    }
}
