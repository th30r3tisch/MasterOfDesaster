using Game_Server.EA.Models;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    abstract class KI_Base: User {

        protected GameManager gm { get; set; }
        protected int tickLength;
        protected int protocollTime;
        protected Individual i;

        public KI_Base(GameManager gm, int kiId, string name, Color color) {
            id = kiId;
            tickLength = (int)(Constants.TOWN_GROTH_SECONDS * 1000 + 10);
            protocollTime = tickLength * 5;
            this.gm = gm;
            SetupUser(name, color);
        }

        /// <summary>
        /// adds the ki to the game and starts the task
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <param name="i">individual</param>
        /// <returns>task with individual</returns>
        public Task<Individual> SendIntoGame(CancellationToken ct, Individual i) {
            this.i = i;
            Server.kis.Add(Server.kis.Count, this);
            return Task.Run(() => PlayAsync(ct));
        }

        /// <summary>
        /// includes the play routine of the ki
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>task with individual</returns>
        protected abstract Task<Individual> PlayAsync(CancellationToken ct);

        protected void CheckKITownLifes(Town town) {
            town.CalculateLife(DateTime.Now);
            if (town.life <= 0) {
                town.life = 0;
                for (int i = town.outgoing.Count; i > 0; i--) {
                    RetreatFromTown(town.position, town.outgoing[i - 1].position, DateTime.Now);
                }
            }
            lock (gm.treeLock) {
                foreach (Town t in town.outgoing) {
                    t.CalculateLife(DateTime.Now);
                    if (t.life <= 0) {
                        t.life = 0;
                        ConquerTown(player, t.position, DateTime.Now);
                        i.score += 20;
                        return;
                    }
                    else if (t.life > i.gene.properties["supportMaxCap"]) {
                        RetreatFromTown(town.position, t.position, DateTime.Now);
                    }
                }
            }
        }

        public override void Disconnect() {
            throw new NotImplementedException();
        }
    }
}
