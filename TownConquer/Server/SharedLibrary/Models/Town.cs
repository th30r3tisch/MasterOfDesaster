using System;
using System.Collections.Generic;
using System.Numerics;

namespace SharedLibrary.Models {

    public class Town : TreeNode {
        public Player owner;
        public double life;
        public long livingTime;
        public TownCategory townCategory;

        public List<Town> incomingAttackerTowns = new List<Town>();
        public List<Town> incomingSupporterTowns = new List<Town>();
        public List<Town> outgoingActionsToTowns = new List<Town>();
        public List<Town> townsInRange = new List<Town>();

        public Town(Vector3 spawnPos) {
            position = spawnPos;
            life = Constants.TOWN_INITIAL_LIFE;
            incomingAttackerTowns = new List<Town>();
        }

        public void RemoveAttackTown(Town town) {
            if (incomingAttackerTowns.Count > 0) {
                incomingAttackerTowns.Remove(town);
            }
        }

        public void RemoveSupporterTown(Town town) {
            if (incomingSupporterTowns.Count > 0) {
                incomingSupporterTowns.Remove(town);
            }
        }

        public void RemoveOutgoingTown(Town town) {
            if (outgoingActionsToTowns.Count > 0) {
                outgoingActionsToTowns.Remove(town);
            }
        }

        /// <summary>
        /// Removes the attack or support reference between two towns
        /// </summary>
        /// <param name="atkTown">the origin of the action</param>
        public void RmTownActionReference(Town atkTown) {
            if (atkTown.owner == owner) {
                RemoveSupporterTown(atkTown);
            }
            else {
                RemoveAttackTown(atkTown);
            }
            atkTown.RemoveOutgoingTown(this);
        }

        /// <summary>
        /// Updates the owner of a town when conquered
        /// </summary>
        /// <param name="newPlayer">The player who conquered the town</param>
        public void UpdateOwner(Player newPlayer, long timestamp) {
            owner.towns.Remove(this);
            livingTime = timestamp;
            owner = newPlayer;
            life = 0;
            owner.towns.Add(this);
        }

        /// <summary>
        /// Adds the attack or support reference between two towns
        /// </summary>
        /// <param name="atkTown">the origin of the action</param>
        public void AddTownActionReference(Town atkTown) {
            if (atkTown.owner == owner) {
                if (!incomingSupporterTowns.Contains(atkTown)) {
                    incomingSupporterTowns.Add(atkTown);
                }
            }
            else {
                if (!incomingAttackerTowns.Contains(atkTown)) {
                    incomingAttackerTowns.Add(atkTown);
                }
            }
            atkTown.outgoingActionsToTowns.Add(this);
        }

        /// <summary>
        /// Checks if town needs support
        /// </summary>
        /// <param name="threshold">maximal life a town can have to qualify for support</param>
        /// <returns>wether town is able to receive support</returns>
        public bool NeedSupport(int threshold) {
            if (life < threshold) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if town can support another town
        /// </summary>
        /// <param name="threshold">minimal life the town needs to support</param>
        /// <returns>wether town is able to support the town</returns>
        public bool CanSupport(int threshold) {
            if (life > threshold && outgoingActionsToTowns.Count < 2) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if town is able to attack other towns
        /// </summary>
        /// <param name="threshold">minimal life the town needs to attack</param>
        /// <returns>wether town is able to attack</returns>
        public bool CanAttack(int threshold) {
            if (life > threshold && outgoingActionsToTowns.Count < 2) {
                return true;
            }
            return false;
        }

        public void CalculateLife(long currentTime, string from) {
            long timePassed = currentTime - livingTime;

            if (timePassed > Constants.KI_TICK_RATE) {
                long timeOverflow = timePassed % Constants.KI_TICK_RATE;
                int pastTickNumber = (int)timePassed / Constants.KI_TICK_RATE;
                double firstLifeCalc = life;
                //Console.WriteLine($"{timePassed} - {livingTime} - {owner.username} - {position} - {incomingAttackerTowns.Count} - {incomingSupporterTowns.Count} - {outgoingActionsToTowns.Count} - {life} - {from}");
                if (owner.id != -1) {
                    int lostLifeByOutgoing = pastTickNumber * outgoingActionsToTowns.Count;
                    int gotLifeByIncoming = pastTickNumber * incomingSupporterTowns.Count;
                    if (incomingAttackerTowns.Count <= 0) { // just generate life when no attacks are incoming
                        firstLifeCalc += pastTickNumber;
                    }
                    firstLifeCalc -= lostLifeByOutgoing + gotLifeByIncoming;
                }
                int lostLifeByIncoming = pastTickNumber * incomingAttackerTowns.Count;

                life = firstLifeCalc - lostLifeByIncoming;
                livingTime = currentTime - timeOverflow;
            }
        }
    }
}
