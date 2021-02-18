﻿using Game_Server.EA.Models.Simple;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    class KI_1 : KI_Base<Individual_Simple> {

        public KI_1(Game gm, int id, string name, Color color) : base(gm, id, name, color) { }

        /// <summary>
        /// includes the play routine of the ki
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>task with individual</returns>
        protected override async Task<Individual_Simple> PlayAsync(CancellationToken ct) {
            indi.startPos = player.towns[0].position;
            var startTickCount = Environment.TickCount;
            int timePassed = 0;
            int townCount = 0;

            while (Constants.TOWN_NUMBER * 0.8 > player.towns.Count) { //  && player.towns.Count != 0
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
                    indi.score -= 20;
                }
                lock (game.gm.treeLock) {
                    for (int x = player.towns.Count; x > 0; x--) {
                        Town town = player.towns[x - 1];
                        CheckKITownLifes(town, indi.gene.properties);
                        if (HasSupportPermission(town)) {
                            TrySupportTown(town);
                        }
                        else {
                            TryAttackTown(town);
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
                    indi.won = false;
                    CalcTownLifeSum();
                    return indi;
                }
            }
            indi.won = true;
            CalcTownLifeSum();
            ProtocollStats(timePassed + Environment.TickCount - startTickCount);
            return indi;
        }

        protected override void CheckKITownLifes(Town town, Dictionary<string, int> props) {
            town.CalculateLife(DateTime.Now);
            if (town.life <= 0) {
                town.life = 0;
                for (int i = town.outgoingActionsToTowns.Count; i > 0; i--) {
                    RetreatFromTown(town.position, town.outgoingActionsToTowns[i - 1].position, DateTime.Now);
                }
            }
            lock (game.gm.treeLock) {
                for (int x = town.outgoingActionsToTowns.Count; x > 0; x--) {
                    Town t = town.outgoingActionsToTowns[x - 1];
                    t.CalculateLife(DateTime.Now);
                    if (t.life <= 0) {
                        t.life = 0;
                        ConquerTown(player, t.position, DateTime.Now);
                        indi.score += 5;
                    }
                    else if (t.life > props["SupportMaxCap"] && t.incomingSupporterTowns.Contains(town)) {
                        RetreatFromTown(town.position, t.position, DateTime.Now);
                    }
                }
            }
        }

        private void TrySupportTown(Town atkTown) {
            List<Town> ownTowns = player.towns;
            foreach (Town supptown in ownTowns) {
                if (game.gm.CanTownsInteract(supptown, atkTown) && supptown.NeedSupport(indi.gene.properties["SupportMinCap"])) {
                    InteractWithTown(atkTown.position, supptown.position, DateTime.Now);
                }
                if (!atkTown.CanSupport(indi.gene.properties["SupportMaxCap"])) {
                    return;
                }
            }   
        }

        private void TryAttackTown(Town atkTown) {
            if (atkTown.CanAttack(indi.gene.properties["AttackMinLife"])) {
                Town deffTown = GetPossibleAttackTarget(atkTown);
                
                if (deffTown != null) {
                    InteractWithTown(atkTown.position, deffTown.position, DateTime.Now);
                }
            }
        }

        /// <summary>
        /// checks if a town has support permissions
        /// </summary>
        /// <param name="town">the town to check</param>
        /// <returns>if the town has permissions</returns>
        private bool HasSupportPermission(Town town) {
            int supportRadius = indi.gene.properties["SupportRadius"];
            int friendlyTownNumber = 0;
            int hostileTownNumber = 0;

            if (!town.CanSupport(indi.gene.properties["SupportMaxCap"])) {
                return false;
            }

            List<TreeNode> objects = game.tree.GetAllContentBetween(
                (int)town.position.X - supportRadius,
                (int)town.position.Z - supportRadius,
                (int)town.position.X + supportRadius,
                (int)town.position.Z + supportRadius);
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
            if (friendlyPercent >= (indi.gene.properties["SupportTownRatio"] / 100f) && allTowns > 1) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// searches an attack target for a town
        /// </summary>
        /// <param name="atkTown">town who has permission to attack</param>
        /// <returns>one random town to attack or null</returns>
        private Town GetPossibleAttackTarget(Town atkTown) {
            int conquerRadius = indi.gene.properties["InitialConquerRadius"];
            QuadTree tree = game.tree;
            
            while (conquerRadius < indi.gene.properties["MaxConquerRadius"] && conquerRadius > 0) {
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
                        if (game.gm.CanTownsInteract(deffTown, atkTown) && deffTown.owner != atkTown.owner) {
                            enemyTowns.Add(deffTown);
                        }
                    }
                }
                if (enemyTowns.Count > 0) {
                    return enemyTowns[r.Next(0, enemyTowns.Count)];
                }
                else conquerRadius += indi.gene.properties["RadiusExpansionStep"];
            }
            return null;
        }

        private void ProtocollStats(int timePassed) {
            indi.name = player.username;
            indi.timestamp.Add(timePassed);
            indi.townNumberDevelopment.Add(player.towns.Count);
        }

        private void CalcTownLifeSum() {
            int life = 0;
            foreach (Town town in player.towns) {
                life += town.life;
            }
            indi.townLifeSum = life;
        }
    }
}
