using System.Collections.Generic;
using System.Numerics;

namespace SharedLibrary.Models {

    public class Town : TreeNode {
        public Player player;
        public int life;
        public List<Town> attackerTowns;

        public Town(Vector3 _spawnPos) {
            position = _spawnPos;
            life = Constants.TOWN_INITIAL_LIFE;
            attackerTowns = new List<Town>();
        }

        public void RemoveAllConquerors() {
            attackerTowns.Clear();
        }

        public void AddAttackTown(Town _town) {
            if (attackerTowns == null) attackerTowns = new List<Town>();
            attackerTowns.Add(_town);
        }

        public void RemoveAttackTown(Town _town) {
            if (attackerTowns != null && attackerTowns.Count > 0) {
                attackerTowns.Remove(_town);
            }
        }
    }
}
