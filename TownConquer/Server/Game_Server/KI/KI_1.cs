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

        List<Town> reachableTowns = new List<Town>();

        public KI_1(Game gm, int id, string name, Color color) : base(gm, id, name, color) { }

        /// <summary>
        /// includes the play routine of the ki
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>task with individual</returns>
        protected override async Task<Individual_Simple> PlayAsync(CancellationToken ct) {
            indi.startPos = player.towns[0].position;
            GetPossibleInteractionTarget(player.towns[0], indi.gene.properties["ConquerRadius"]);
            var startTickCount = Environment.TickCount;
            int timePassed = 0;
            int townCountOld = 0;

            while (Constants.TOWN_NUMBER * 0.9 > player.towns.Count) { //  && player.towns.Count != 0

                try {
                    await Task.Delay(tickLength);
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
                    }

                    for (int x = townCountNew; x > 0; x--) {
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
                    Console.WriteLine($"{player.username} - LOST");
                    if (reachableTowns.Count < player.towns.Count) {
                        Console.WriteLine($"ALERT **********************************************************************");
                    }
                    CalcTownLifeSum();
                    return indi;
                }
            }
            indi.won = true;
            Console.WriteLine($"{player.username} - WON");
            if (reachableTowns.Count < player.towns.Count) {
                Console.WriteLine($"ALERT **********************************************************************");
            }
            CalcTownLifeSum();
            ProtocollStats(timePassed + Environment.TickCount - startTickCount);
            return indi;
        }

        protected override void CheckKITownLifes(Town town, Dictionary<string, int> props) {
            town.CalculateLife(DateTime.Now);
            if (town.life <= 0) {
                for (int i = town.outgoingActionsToTowns.Count; i > 0; i--) {
                    RetreatFromTown(town.position, town.outgoingActionsToTowns[i - 1].position, DateTime.Now);
                }
                town.life = 0;
            }

            for (int x = town.outgoingActionsToTowns.Count; x > 0; x--) {
                Town t = town.outgoingActionsToTowns[x - 1];
                t.CalculateLife(DateTime.Now);
                Console.WriteLine($"{t.position} - {t.life}");
                if (t.life <= 0) {
                    ConquerTown(player, t.position, DateTime.Now);
                    indi.score += 20;
                    t.life = 0;
                    GetPossibleInteractionTarget(t, indi.gene.properties["ConquerRadius"]);
                }
                else if (t.life > props["SupportMaxCap"] && t.incomingSupporterTowns.Contains(town)) {
                    RetreatFromTown(town.position, t.position, DateTime.Now);
                }
            }
        }

        private void TrySupportTown(Town sourceTown) {
            if (sourceTown.townsInRange.Count <= 0) return;
            foreach (Town town in sourceTown.townsInRange) {
                if (!sourceTown.CanSupport(indi.gene.properties["SupportMaxCap"])) {
                    return;
                }
                if (town.owner.username.Equals(sourceTown.owner.username) &&
                    town.NeedSupport(indi.gene.properties["SupportMinCap"])) {
                    InteractWithTown(sourceTown.position, town.position, DateTime.Now);
                }
            }
        }

        private void TryAttackTown(Town sourceTown) {
            if (sourceTown.townsInRange.Count <= 0) return;
            foreach (Town town in sourceTown.townsInRange) {
                if (!sourceTown.CanAttack(indi.gene.properties["AttackMinLife"])) {
                    return;
                }
                if (!town.owner.username.Equals(sourceTown.owner.username)) {
                    InteractWithTown(sourceTown.position, town.position, DateTime.Now);
                }
            }
        }

        /// <summary>
        /// checks if a town has support permissions
        /// </summary>
        /// <param name="town">the town to check</param>
        /// <returns>if the town has permissions</returns>
        private bool HasSupportPermission(Town town) {
            int friendlyTownNumber = 0;
            int hostileTownNumber = 0;

            if (!town.CanSupport(indi.gene.properties["SupportMaxCap"])) {
                return false;
            }
            foreach (Town t in town.townsInRange) {
                if (t.owner == player) {
                    friendlyTownNumber++;
                }
                else {
                    hostileTownNumber++;
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
        /// searches neighbours of a town for a town
        /// </summary>
        /// <param name="atkTown">town possible interaction targets</param>
        /// <param name="searchRadius">radius around town to search for targets</param>
        private void GetPossibleInteractionTarget(Town atkTown, int searchRadius) {
            QuadTree tree = game.tree;
            List<TreeNode> objectsInRange;

            objectsInRange = tree.GetAllContentBetween(
                (int)(atkTown.position.X - searchRadius),
                (int)(atkTown.position.Z - searchRadius),
                (int)(atkTown.position.X + searchRadius),
                (int)(atkTown.position.Z + searchRadius));

            for (int i = 0; i < objectsInRange.Count; i++) {
                if (objectsInRange[i] is Town town) {
                    if (game.gm.CanTownsInteract(town, atkTown)) {
                        atkTown.townsInRange.Add(town);
                        if (!reachableTowns.Contains(town)) {
                            reachableTowns.Add(town);
                            Console.WriteLine($"{player.username} - {reachableTowns.Count}");
                        }
                    }
                    else {
                        Console.WriteLine($"{player.username} - NO INTERACTION");
                    }
                }
            }
        }

        private void ProtocollStats(int timePassed) {
            indi.name = player.username;
            indi.timestamp.Add(timePassed);
            indi.townNumberDevelopment.Add(player.towns.Count);
        }

        private void CalcTownLifeSum() {
            double life = 0;
            foreach (Town town in player.towns) {
                life += town.life;
            }
            indi.townLifeSum = life;
        }
    }
}
