using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Threading;

namespace Game_Server.KI {
    abstract class KI_base {

        public Player player { get; set; }
        public QuadTree world { get; set; }

        public Thread kiThread;

        public KI_base(QuadTree _world) {
            world = _world;
            kiThread = new Thread(Run);
        }

        public abstract void Run(object logic);

        protected void CheckKITownLifes(Town _town, GameLogic logic) {
            logic.CalculateTownLife(_town, DateTime.Now);
            if (_town.life <= 0) {
                _town.life = 0;
                for (int i = _town.outgoing.Count; i > 0; i--) {
                    logic.RemoveAttackFromTown(_town.position, _town.outgoing[i-1].position, DateTime.Now);
                }
            }
            foreach (Town _t in _town.outgoing) {
                logic.CalculateTownLife(_t, DateTime.Now);
                if (_t.life <= 0) {
                    _town.life = 0;
                    logic.ConquerTown(player, _t.position, DateTime.Now);
                    if (Constants.TRAININGS_MODE == false) {
                        foreach (Client _client in Server.clients.Values) {
                            if (_client.player != null) {
                                ServerSend.GrantedConquer(_client.id, player, _t.position);
                            }
                        }
                    }
                    return;
                }
            }
        }
    }
}
