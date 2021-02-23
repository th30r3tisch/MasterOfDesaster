using Game_Server.EA.Models;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    abstract class KI_Base<T>: User where T: IIndividual  {

        protected long protocollTime; //millisec
        protected T indi;

        public KI_Base(Game game, int kiId, string name, Color color) {
            id = kiId;
            protocollTime = Constants.KI_TICK_RATE * 5;
            this.game = game;
            SetupUser(name, color, game.gm.sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// adds the ki to the game and starts the task
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <param name="i">individual</param>
        /// <returns>task with individual</returns>
        public Task<T> SendIntoGame(CancellationToken ct, T i) {
            indi = i;
            game.kis.Add(game.kis.Count, this);
            return Task.Run(() => PlayAsync(ct));
        }

        /// <summary>
        /// includes the play routine of the ki
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>task with individual</returns>
        protected abstract Task<T> PlayAsync(CancellationToken ct);

        protected abstract void CheckKITownLifes(Town town, Dictionary<string, int> props);

        public override void Disconnect() {
            throw new NotImplementedException();
        }
    }
}
