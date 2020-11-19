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

        public abstract void Run();

        protected void CheckKITownLifes(Town _town) {
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
    }
}
