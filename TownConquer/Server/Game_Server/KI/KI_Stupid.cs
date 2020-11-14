
using SharedLibrary.Models;
using System;
using System.Collections.Generic;

namespace Game_Server.KI {
    class KI_Stupid {

        public void GetPossibleAttackTarget(Town _t, QuadTree _quadTree) {
            int _conquerRadius = 400;
            Town _target = null;

            while (_target == null || _conquerRadius < 700) {
                List<TreeNode> _townsInRange;
                List<Town> _enemyTowns = new List<Town>();
                Random r = new Random();
                _townsInRange = _quadTree.GetAllContentBetween(
                    (int)(_t.position.X - _conquerRadius),
                    (int)(_t.position.Z - _conquerRadius),
                    (int)(_t.position.X + _conquerRadius),
                    (int)(_t.position.Z + _conquerRadius));

                foreach (Town _town in _townsInRange) {
                    if (!_town.player.username.Equals("KI")) {
                        _enemyTowns.Add(_town);
                    }
                }
                if (_enemyTowns.Count > 0) {

                    //attackRequest(_t, _enemyTowns[r.Next(0, _enemyTowns.Count)]);
                }
                else _conquerRadius += 100;
            }
        }

        public void GetPossibleSupportTarget(Town _t, QuadTree _quadTree) {
            int _supportRadius = 400;
        }
    }
}
