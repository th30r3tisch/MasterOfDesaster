using Game_Server.KI.Models;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    abstract class KI_base {

        public Player player { get; set; }
        public GameManager gm { get; set; }

        protected int tickLength;
        protected int protocollTime;
        protected Individual i;

        public KI_base(GameManager _gm) {
            tickLength = (int)(Constants.TOWN_GROTH_SECONDS * 1000 + 10);
            protocollTime = tickLength * 5;
            gm = _gm;
        }

        public Task<Individual> Start(CancellationToken _ct, Individual _i) {
            i = _i;
            gm.game.kis.Add(this);
            return Task.Run(() => PlayAsync(_ct));
        }

        public abstract Task<Individual> PlayAsync(CancellationToken ct);

        protected void CheckKITownLifes(Town _town) {
            gm.CalculateTownLife(_town, DateTime.Now);
            if (_town.life <= 0) {
                _town.life = 0;
                for (int i = _town.outgoing.Count; i > 0; i--) {
                    gm.RemoveAttackFromTown(_town.position, _town.outgoing[i-1].position, DateTime.Now);
                }
            }
            lock (gm.treeLock) {
                foreach (Town _t in _town.outgoing) {
                    gm.CalculateTownLife(_t, DateTime.Now);
                    if (_t.life <= 0) {
                        _town.life = 0;
                        gm.ConquerTown(player, _t.position, DateTime.Now);
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
}
