using Game_Server.EA.Models;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    abstract class KI_Base<T>: User where T: IIndividual  {

        protected GameManager gm { get; set; }
        protected int tickLength;
        protected int protocollTime;
        protected T i;

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
        public Task<T> SendIntoGame(CancellationToken ct, T i) {
            this.i = i;
            Server.kis.Add(Server.kis.Count, this);
            return Task.Run(() => PlayAsync(ct));
        }

        /// <summary>
        /// includes the play routine of the ki
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>task with individual</returns>
        protected abstract Task<T> PlayAsync(CancellationToken ct);

        protected abstract void CheckKITownLifes(Town town);

        public override void Disconnect() {
            throw new NotImplementedException();
        }
    }
}
