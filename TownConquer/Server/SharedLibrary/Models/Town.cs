using System;
using System.Collections.Generic;
using System.Numerics;

namespace SharedLibrary.Models {

    public class Town : TreeNode {
        public Player player;
        public int life;
        public DateTime creationTime;

        public List<Town> attackerTowns = new List<Town>();
        public List<Town> supporterTowns = new List<Town>();
        public List<Town> outgoing = new List<Town>();

        public Town(Vector3 spawnPos) {
            position = spawnPos;
            life = Constants.TOWN_INITIAL_LIFE;
            attackerTowns = new List<Town>();
        }

        public void RemoveAttackTown(Town town) {
            if (attackerTowns.Count > 0) {
                attackerTowns.Remove(town);
            }
        }

        public void RemoveSupporterTown(Town town) {
            if (supporterTowns.Count > 0) {
                supporterTowns.Remove(town);
            }
        }

        public void RemoveOutgoingTown(Town town) {
            if (outgoing.Count > 0) {
                outgoing.Remove(town);
            }
        }

        public void CalculateLife(DateTime currentTime) {
            TimeSpan span = currentTime.Subtract(creationTime);
            float timePassed = (float)span.TotalSeconds;
            int firstLifeCalc = life;

            if (player.id != -1) {
                int rawTownLife = (int)(timePassed / Constants.TOWN_GROTH_SECONDS);
                int lostLifeByOutgoing = (int)(timePassed / Constants.TOWN_GROTH_SECONDS * outgoing.Count);
                int gotLifeByIncoming = (int)(timePassed / Constants.TOWN_GROTH_SECONDS * supporterTowns.Count);
                firstLifeCalc += rawTownLife - lostLifeByOutgoing + gotLifeByIncoming;
            }
            int lostLifeByIncoming = (int)(timePassed / Constants.TOWN_GROTH_SECONDS * attackerTowns.Count);

            int finalNewLife = firstLifeCalc - lostLifeByIncoming;
            life = finalNewLife;
            creationTime = currentTime;
        }
    }
}
