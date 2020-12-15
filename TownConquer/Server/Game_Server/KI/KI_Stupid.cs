using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Game_Server.KI {
    class KI_Stupid : KI_base {

        public KI_Stupid(QuadTree _world, int id, string name, Color color, GameLogic logic) : base(_world) {
            player = new Player(id, name, color, DateTime.Now);
            Town _t = logic.CreateTown(player);
            _t.player = player;
            kiThread.Start(logic);
            Console.WriteLine($"KI_Stupid started.");
        }

        public override void Run(object logic) {
            while (true) {
                try {
                    Thread.Sleep((int)(Constants.TOWN_GROTH_SECONDS * 1000 + 10));
                }
                catch (Exception _ex) {
                    Console.WriteLine($"KI_Stupid error: {_ex}");
                }
                for(int i = player.towns.Count; i > 0; i--) {
                    Town _atkTown = player.towns[i - 1];
                    CheckKITownLifes(_atkTown, (GameLogic)logic);
                    TryAttackTown(_atkTown, (GameLogic)logic);
                }
            }
        }

        private void TryAttackTown(Town _atkTown, GameLogic logic) {
            if (_atkTown.life > 10 && _atkTown.outgoing.Count < 2) {
                Town _deffTown = GetPossibleAttackTarget(_atkTown, world, logic);
                if (_deffTown != null) {
                    logic.AddAttackToTown(_atkTown.position, _deffTown.position, DateTime.Now);
                    if (Constants.TRAININGS_MODE == false) {
                        foreach (Client _client in Server.clients.Values) {
                            if (_client.player != null) {
                                ServerSend.GrantedAttack(_client.id, _atkTown.position, _deffTown.position);
                            }
                        }
                    }
                }
            }
        }

        private Town GetPossibleAttackTarget(Town _atkTown, QuadTree _quadTree, GameLogic logic) {
            int _conquerRadius = 400;
            Town _target = null;

            while (_target == null && _conquerRadius < 2000) {
                List<TreeNode> _townsInRange;
                List<Town> _enemyTowns = new List<Town>();
                Random _r = new Random();
                _townsInRange = _quadTree.GetAllContentBetween(
                    (int)(_atkTown.position.X - _conquerRadius),
                    (int)(_atkTown.position.Z - _conquerRadius),
                    (int)(_atkTown.position.X + _conquerRadius),
                    (int)(_atkTown.position.Z + _conquerRadius));

                for(int i = 0; i < _townsInRange.Count; i++) {
                    if (_townsInRange[i] is Town _deffTown) {
                        if (!_deffTown.player.username.Equals(_atkTown.player.username) &&
                        !logic.IsIntersecting(_atkTown.position, _deffTown.position) &&
                        !_atkTown.outgoing.Contains(_deffTown)) {
                            _enemyTowns.Add(_deffTown);
                        }
                    }
                }
                if (_enemyTowns.Count > 0) {
                    return _enemyTowns[_r.Next(0, _enemyTowns.Count - 1)];
                }
                else _conquerRadius += 100;
            }
            return null;
        }

        public void GetPossibleSupportTarget(Town _t, QuadTree _quadTree) {
            int _supportRadius = 400;
        }
    }
}
