
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Game_Server.KI {
    class KI_Stupid : KI_base {

        public KI_Stupid(QuadTree _world, string name, Color color) : base(_world) {
            player = new Player(999, name, color, DateTime.Now);
            Town _t = GameLogic.CreateTown(player);
            _t.player = player;
            kiThread.Start();
            Console.WriteLine($"KI_Stupid started.");
        }

        public override void Run() {
            while (true) {
                try {
                    Thread.Sleep(2100);
                }
                catch (Exception _ex) {
                    Console.WriteLine($"KI_Stupid error: {_ex}");
                }
                for(int i = player.towns.Count; i > 0; i--) {
                    Town _atkTown = player.towns[i - 1];
                    CheckKITownLifes(_atkTown);
                    if (_atkTown.life > 20 && _atkTown.outgoing.Count < 1) {
                        Town _deffTown = GetPossibleAttackTarget(_atkTown, world);
                        if (_deffTown != null) {
                            GameLogic.AddAttackToTown(_atkTown.position, _deffTown.position, DateTime.Now);
                            foreach (Client _client in Server.clients.Values) {
                                if (_client.player != null) {
                                    ServerSend.GrantedAttack(_client.id, _atkTown.position, _deffTown.position);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckKITownLifes(Town _town) {
            GameLogic.CalculateTownLife(_town, DateTime.Now);
            foreach (Town _t in _town.outgoing) {
                GameLogic.CalculateTownLife(_t, DateTime.Now);
                if (_t.life <= 0) {
                    GameLogic.ConquerTown(player, _t.position, DateTime.Now);
                    foreach (Client _client in Server.clients.Values) {
                        if (_client.player != null) {
                            ServerSend.GrantedConquer(_client.id, player, _t.position);
                        }
                    }
                    return;
                }
            } 
        }

        public static Town GetPossibleAttackTarget(Town _atkTown, QuadTree _quadTree) {
            int _conquerRadius = 400;
            Town _target = null;

            while (_target == null && _conquerRadius < 1500) {
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
                        if (!_deffTown.player.username.Equals("KI") &&
                        !GameLogic.IsIntersecting(_atkTown.position, _deffTown.position)) {
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
