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

        public Town(Vector3 _spawnPos) {
            position = _spawnPos;
            life = Constants.TOWN_INITIAL_LIFE;
            attackerTowns = new List<Town>();
        }

        public void AddAttackTown(Town _town) {
            attackerTowns.Add(_town);
        }

        public void RemoveAttackTown(Town _town) {
            if (attackerTowns.Count > 0) {
                attackerTowns.Remove(_town);
            }
        }

        public void AddSupporterTown(Town _town) {
            supporterTowns.Add(_town);
        }

        public void RemoveSupporterTown(Town _town) {
            if (supporterTowns.Count > 0) {
                supporterTowns.Remove(_town);
            }
        }

        public void AddOutgoingTown(Town _town) {
            outgoing.Add(_town);
        }

        public void RemoveOutgoingTown(Town _town) {
            if (outgoing.Count > 0) {
                outgoing.Remove(_town);
            }
        }
    }
}
