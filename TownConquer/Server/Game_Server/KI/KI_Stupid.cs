using Game_Server.KI.Models;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    class KI_Stupid : KI_base {

        public KI_Stupid(GameManager _gm, int id, string name, Color color) : base(_gm) {
            player = new Player(id, name, color, DateTime.Now);
            Town _t = gm.CreateTown(player);
            _t.player = player;
        }

        public override async Task<Individual> PlayAsync(CancellationToken ct) {
            Console.WriteLine($"{player.username} started.");
            while (Constants.TOWN_NUMBER*0.8 > player.towns.Count || player.towns.Count == 0) {
                try {
                    await Task.Delay((int)(Constants.TOWN_GROTH_SECONDS * 1000 + 10));
                }
                catch (Exception _ex) {
                    Console.WriteLine($"{player.username} error: {_ex}");
                }
                lock (gm.treeLock) {
                    for (int i = player.towns.Count; i > 0; i--) {
                        Town _atkTown = player.towns[i - 1];
                        CheckKITownLifes(_atkTown);
                        TryAttackTown(_atkTown);
                    }
                }
                if (ct.IsCancellationRequested) {
                    return i;
                }
            }
            if (Constants.TOWN_NUMBER * 0.8 <= player.towns.Count) {
                winner = true;
                return i;
            }
            return i;
        }

        private void TryAttackTown(Town _atkTown) {
            if (_atkTown.life > 10 && _atkTown.outgoing.Count < 2) {
                Town _deffTown = GetPossibleAttackTarget(_atkTown);
                if (_deffTown != null) {
                    gm.AddAttackToTown(_atkTown.position, _deffTown.position, DateTime.Now);
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

        private Town GetPossibleAttackTarget(Town _atkTown) {
            int _conquerRadius = i.gene.initialConquerRadius;
            Town _target = null;
            QuadTree tree = gm.game.tree;

            while (_target == null && _conquerRadius < i.gene.maxConquerRadius) {
                List<TreeNode> _townsInRange;
                List<Town> _enemyTowns = new List<Town>();
                Random _r = new Random();
                _townsInRange = tree.GetAllContentBetween(
                    (int)(_atkTown.position.X - _conquerRadius),
                    (int)(_atkTown.position.Z - _conquerRadius),
                    (int)(_atkTown.position.X + _conquerRadius),
                    (int)(_atkTown.position.Z + _conquerRadius));

                for(int i = 0; i < _townsInRange.Count; i++) {
                    if (_townsInRange[i] is Town _deffTown) {
                        if (!_deffTown.player.username.Equals(_atkTown.player.username) &&
                        !gm.IsIntersecting(_atkTown.position, _deffTown.position) &&
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
