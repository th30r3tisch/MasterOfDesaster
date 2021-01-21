using Game_Server.EA.Models;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    abstract class KI_base {

        public Player player { get; set; }

        protected GameManager gm { get; set; }
        protected int tickLength;
        protected int protocollTime;
        protected Individual i;

        public KI_base(GameManager gm) {
            tickLength = (int)(Constants.TOWN_GROTH_SECONDS * 1000 + 10);
            protocollTime = tickLength * 5;
            this.gm = gm;
        }

        /// <summary>
        /// adds the ki to the game and starts the task
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <param name="i">individual</param>
        /// <returns>task with individual</returns>
        public Task<Individual> Start(CancellationToken ct, Individual i) {
            this.i = i;
            gm.game.kis.Add(this);
            return Task.Run(() => PlayAsync(ct));
        }

        /// <summary>
        /// includes the play routine of the ki
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>task with individual</returns>
        protected abstract Task<Individual> PlayAsync(CancellationToken ct);

        protected void CheckKITownLifes(Town town) {
            gm.CalculateTownLife(town, DateTime.Now);
            if (town.life <= 0) {
                town.life = 0;
                for (int i = town.outgoing.Count; i > 0; i--) {
                    gm.RemoveAttackFromTown(town.position, town.outgoing[i - 1].position, DateTime.Now);
                }
            }
            lock (gm.treeLock) {
                foreach (Town t in town.outgoing) {
                    gm.CalculateTownLife(t, DateTime.Now);
                    if (t.life <= 0) {
                        town.life = 0;
                        gm.ConquerTown(player, t.position, DateTime.Now);
                        i.score += 20;
                        if (Constants.TRAININGS_MODE == false) {
                            foreach (Client client in Server.clients.Values) {
                                if (client.player != null) {
                                    ServerSend.GrantedConquer(client.id, player, t.position);
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
