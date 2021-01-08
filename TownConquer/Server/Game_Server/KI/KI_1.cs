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

        protected override async Task<Individual> PlayAsync(CancellationToken ct) {
            Console.WriteLine($"{player.username} started.");
            i.startPos = player.towns[0].position;
            var startTickCount = Environment.TickCount;
            int timePassed = 0;

            while (Constants.TOWN_NUMBER * 0.8 > player.towns.Count || player.towns.Count == 0) {
                try {
                    await Task.Delay(tickLength);
                }
                catch (Exception _ex) {
                    Console.WriteLine($"{player.username} error: {_ex}");
                }
                lock (gm.treeLock) {
                    for (int i = player.towns.Count; i > 0; i--) {
                        Town _atkTown = player.towns[i - 1];
                        CheckKITownLifes(_atkTown);
                        TryAttackTown(_atkTown);
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
            return i;
        }

        private void ProtocollStats(int timePassed) {
            i.name = player.username;
            i.result.timestamp.Add(timePassed);
            i.result.townNumberDevelopment.Add(player.towns.Count);
        }

        private void TryAttackTown(Town atkTown) {
            if (atkTown.life > 10 && atkTown.outgoing.Count < 2) {
                Town deffTown = GetPossibleAttackTarget(atkTown);
                if (deffTown != null) {
                    gm.AddAttackToTown(atkTown.position, deffTown.position, DateTime.Now);
                    if (Constants.TRAININGS_MODE == false) {
                        foreach (Client client in Server.clients.Values) {
                            if (client.player != null) {
                                ServerSend.GrantedAttack(client.id, atkTown.position, deffTown.position);
                            }
                        }
                    }
                }
            }
        }

        private Town GetPossibleAttackTarget(Town atkTown) {
            int conquerRadius = i.gene.initialConquerRadius;
            Town target = null;
            QuadTree tree = gm.game.tree;

            while (target == null && conquerRadius < i.gene.maxConquerRadius) {
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
                else conquerRadius += 100;
            }
            return null;
        }

        public void GetPossibleSupportTarget(Town t, QuadTree quadTree) {
            int supportRadius = 400;
        }
    }
}
