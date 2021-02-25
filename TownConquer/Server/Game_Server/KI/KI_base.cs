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
            protocollTime = Constants.KI_TICK_RATE * 2;
            this.game = game;
            SetupUser(name, color, this.game.gm.sw.ElapsedMilliseconds);
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
        /// searches neighbours of a town for a town
        /// </summary>
        /// <param name="atkTown">town possible interaction targets</param>
        /// <param name="searchRadius">radius around town to search for targets</param>
        protected void GetPossibleInteractionTarget(Town atkTown, int searchRadius) {
            QuadTree tree = game.tree;
            List<TreeNode> objectsInRange;

            objectsInRange = tree.GetAllContentBetween(
                (int)(atkTown.position.X - searchRadius),
                (int)(atkTown.position.Z - searchRadius),
                (int)(atkTown.position.X + searchRadius),
                (int)(atkTown.position.Z + searchRadius));

            for (int i = 0; i < objectsInRange.Count; i++) {
                if (objectsInRange[i] is Town town) {
                    if (game.gm.CanTownsInteract(town, atkTown)) {
                        atkTown.townsInRange.Add(town);
                    }
                }
            }
        }

        /// <summary>
        /// includes the play routine of the ki
        /// </summary>
        /// <param name="ct">CancellationToken</param>
        /// <returns>task with individual</returns>
        protected abstract Task<T> PlayAsync(CancellationToken ct);

        protected abstract void CheckKITownLifes(Town town, Dictionary<string, int> props);

        public abstract override void Disconnect();
    }
}
