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

        public KI_1(Game game, int id, string name, Color color) : base(game, id, name, color) { }

        /// <summary>
        /// includes the play routine of the ki
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>task with individual</returns>
        protected override async Task<Individual_Simple> PlayAsync(CancellationToken ct) {
            indi.startPos = player.towns[0].position;

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
                    indi.score += 20;
                    GetPossibleInteractionTarget(t, indi.gene.properties["ConquerRadius"]);
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

        /// <summary>
        /// tries to attack a town
        /// </summary>
        /// <param name="sourceTown">town that wants to attack</param>
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
        /// loggs the data during the game
        /// </summary>
        /// <param name="timePassed">time stamp of the log action</param>
        /// <param name="townNum">town number at the time of the log action</param>
        private void ProtocollStats(long timePassed, int townNum) {
            indi.name = player.username;
            indi.timestamp.Add(timePassed);
            indi.townNumberDevelopment.Add(townNum);
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
            ProtocollStats(game.gm.sw.ElapsedMilliseconds, player.towns.Count);
        }
    }
}
