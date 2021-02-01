using Game_Server.EA.Models;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    class KI_1 : KI_base {

        public KI_1(GameManager gm, int id, string name, Color color) : base(gm) {
            player = new Player(id, name, color, DateTime.Now);
            Town t = base.gm.CreateTown(player);
            t.player = player;
        }

        /// <summary>
        /// includes the play routine of the ki
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>task with individual</returns>
        protected override async Task<Individual> PlayAsync(CancellationToken ct) {
            i.startPos = player.towns[0].position;
            var startTickCount = Environment.TickCount;
            int timePassed = 0;
            int townCount = 0;

            while (Constants.TOWN_NUMBER * 0.8 > player.towns.Count || player.towns.Count == 0) {
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
                        Town _atkTown = player.towns[i - 1];
                        CheckKITownLifes(_atkTown);
                        if (IsSupportTown(_atkTown)) {
                            TrySupportTown(_atkTown);
                        }
                        else {
                            TryAttackTown(_atkTown);
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

        private void TrySupportTown(Town atkTown) {
            List<Town> ownTowns = player.towns;
            foreach (Town supptown in ownTowns) {
                if (supptown.life < i.gene.properties["supportMinCap"]) {
                    if (!gm.IsIntersecting(atkTown.position, supptown.position) &&
                    !atkTown.outgoing.Contains(supptown)) {
                        DoAction(atkTown, supptown);
                    }
                }
            }   
        }

        private void DoAction(Town startTown, Town endtown) {
            gm.AddActionToTown(startTown.position, endtown.position, DateTime.Now);
            if (Constants.TRAININGS_MODE == false) {
                foreach (Client client in Server.clients.Values) {
                    if (client.player != null) {
                        ServerSend.GrantedAction(client.id, startTown.position, endtown.position);
                    }
                }
            }
        }

        private bool IsSupportTown(Town atkTown) {
            int supportRadius = i.gene.properties["supportRadius"];
            int friendlyTownNumber = 0;
            int hostileTownNumber = 0;
            bool isSupTown = false;
            List<TreeNode> objects = gm.game.tree.GetAllContentBetween(
                (int)atkTown.position.X - supportRadius,
                (int)atkTown.position.Z - supportRadius,
                (int)atkTown.position.X + supportRadius,
                (int)atkTown.position.Z + supportRadius);
            foreach (TreeNode node in objects) {
                if (node is Town) {
                    Town t = (Town)node;
                    if (t.player == player) {
                        friendlyTownNumber++;
                    }
                    else {
                        hostileTownNumber++;
                    }
                }
            }
            int friendlyPercent = friendlyTownNumber / (friendlyTownNumber + hostileTownNumber);
            if (friendlyPercent >= i.gene.properties["supportTownRatio"]) {
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
            if (atkTown.life > i.gene.properties["attackMinLife"] && atkTown.outgoing.Count < 2) {
                Town deffTown = GetPossibleAttackTarget(atkTown);
                if (deffTown != null) {
                    DoAction(atkTown, deffTown);
                }
            }
        }

        private Town GetPossibleAttackTarget(Town atkTown) {
            int conquerRadius = i.gene.properties["initialConquerRadius"];
            Town target = null;
            QuadTree tree = gm.game.tree;

            while (target == null && conquerRadius < i.gene.properties["maxConquerRadius"] && conquerRadius > 0) {
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
                else conquerRadius += i.gene.properties["radiusExpansionStep"];
            }
            return null;
        }
    }
}
