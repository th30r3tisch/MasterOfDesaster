using System;
using System.Collections.Generic;
using System.Numerics;

namespace SharedLibrary.Models {

    public class Town : TreeNode {
        public int id;
        public Player player;
        public int life;
        public List<Town> attackerTowns;

        public Town(Vector3 _spawnPos, int _id) {
            position = _spawnPos;
            id = _id;
            life = Constants.TOWN_INITIAL_LIFE;
            attackerTowns = new List<Town>();
        }

        public void RemoveAllConquerors() {
            attackerTowns.Clear();
        }

        public List<Town> GetAttackTowns() {
            List<Town> _towns = new List<Town>();
            if (attackerTowns != null && attackerTowns.Count > 0) {
                foreach (Town _town in attackerTowns) {
                    _towns.Add(_town);
                }
            }
            return _towns;
        }

        public void AddAttackTown(Town _town) {
            if (attackerTowns == null) attackerTowns = new List<Town>();
            attackerTowns.Add(_town);
        }

        public void RemoveAttackTown(Town _town) {
            if (attackerTowns != null && attackerTowns.Count > 0) {
                foreach (Town _entry in attackerTowns) {
                    if (_entry.position.X == _town.position.X && _entry.position.Y == _town.position.Y) {
                        attackerTowns.Remove(_entry);
                    }
                }
            }
        }
    }
}
