using System;
using System.Collections.Generic;
using System.Numerics;

namespace SharedLibrary.Models {

    public class Town : TreeNode {
        public Player owner;
        public double life;
        public DateTime creationTime;
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
                //Console.WriteLine($"SUPP--- {atkTown.owner.username}-{atkTown.position} with life {atkTown.life}-{position} REM");
                RemoveSupporterTown(atkTown);
            }
            else {
                //Console.WriteLine($"ATK--- {atkTown.owner.username}-{atkTown.position} with life {atkTown.life}-{position} REM");
                RemoveAttackTown(atkTown);
            }
            atkTown.RemoveOutgoingTown(this);
        }

        /// <summary>
        /// Updates the owner of a town when conquered
        /// </summary>
        /// <param name="newPlayer">The player who conquered the town</param>
        public void UpdateOwner(Player newPlayer) {
            owner.towns.Remove(this);
            //Console.WriteLine($"CON {newPlayer.username}-{position}");
            creationTime = DateTime.Now;
            owner = newPlayer;
            owner.towns.Add(this);
        }

        /// <summary>
        /// Adds the attack or support reference between two towns
        /// </summary>
        /// <param name="atkTown">the origin of the action</param>
        public void AddTownActionReference(Town atkTown) {
            if (atkTown.owner == owner) {
                if (!incomingSupporterTowns.Contains(atkTown)) {
                    //Console.WriteLine($"SUPP### {atkTown.owner.username}-{atkTown.position} with life {atkTown.life}-{position}");
                    incomingSupporterTowns.Add(atkTown);
                }
            }
            else {
                if (!incomingAttackerTowns.Contains(atkTown)) {
                    //Console.WriteLine($"ATK### {atkTown.owner.username}-{atkTown.position} with life {atkTown.life}-{position}");
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

        public void CalculateLife(DateTime currentTime) {
            TimeSpan span = currentTime.Subtract(creationTime);
            float timePassed = (float)span.TotalSeconds;
            double firstLifeCalc = life;

            if (owner.id != -1) {
                float rawTownLife = (timePassed / Constants.TOWN_GROTH_SECONDS);
                Console.WriteLine($"{position} - {rawTownLife}");
                float lostLifeByOutgoing = (timePassed / Constants.TOWN_GROTH_SECONDS * outgoingActionsToTowns.Count);
                float gotLifeByIncoming = (timePassed / Constants.TOWN_GROTH_SECONDS * incomingSupporterTowns.Count);
                firstLifeCalc += rawTownLife - lostLifeByOutgoing + gotLifeByIncoming;
            }
            float lostLifeByIncoming = (timePassed / Constants.TOWN_GROTH_SECONDS * incomingAttackerTowns.Count);

            double finalNewLife = firstLifeCalc - lostLifeByIncoming;
            life = Math.Round(finalNewLife, 0);
            creationTime = currentTime;
        }
    }
}
