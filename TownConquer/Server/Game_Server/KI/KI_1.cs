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
                    
            indi.townNumberDevelopment.Clear(); // sometimes individuals are not clean at start?
            indi.timestamp.Clear(); // sometimes individuals are not clean at start?

            GetPossibleInteractionTarget(player.towns[0], indi.gene.properties["ConquerRadius"]);
            int townCountOld = 0;
            

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
                long timeMem = game.gm.sw.ElapsedMilliseconds;
                if (timeMem > protocollTime) {
                    protocollTime += timeMem;
                    ProtocollStats(timeMem, player.towns.Count);
                }
                if (ct.IsCancellationRequested) {
                    Disconnect();
                    return indi;
                }
            }
            Disconnect();
            return indi;
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
                    //Console.WriteLine($"{player.username} - {game.id} - {town.position} -> {t.position} - CONQ");
                    ConquerTown(player, t.position);
                    indi.score += 20;
                    GetPossibleInteractionTarget(t, indi.gene.properties["ConquerRadius"]);
                }
                else if (t.life > props["SupportMaxCap"] && t.incomingSupporterTowns.Contains(town)) {
                    RetreatFromTown(town.position, t.position);
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
                    InteractWithTown(sourceTown.position, town.position);
                }
                else {
                    TryAttackTown(sourceTown);
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
                    InteractWithTown(sourceTown.position, town.position);
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
                        }
                    }
                }
            }
        }

        private void ProtocollStats(long timePassed, int townNum) {
            indi.name = player.username;
            indi.timestamp.Add(timePassed);
            indi.townNumberDevelopment.Add(townNum);
        }

        private void CalcTownLifeSum() {
            double life = 0;
            foreach (Town town in player.towns) {
                life += town.life;
            }
            indi.townLifeSum = life;
        }

        public override void Disconnect() {
            if (game.kis[0] != this) {
                indi.won = player.towns.Count > game.kis[0].player.towns.Count;
            }
            else {
                indi.won = player.towns.Count > game.kis[1].player.towns.Count;
            }
            CalcTownLifeSum();
            ProtocollStats(game.gm.sw.ElapsedMilliseconds, player.towns.Count);
        }
    }
}
