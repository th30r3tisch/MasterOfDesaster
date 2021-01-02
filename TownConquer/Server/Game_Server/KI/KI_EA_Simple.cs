using Game_Server.KI.Models;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    class KI_EA_Simple : KI_base {

        public KI_EA_Simple(GameManager _gm, int id, string name, Color color) : base(_gm) {
            player = new Player(id, name, color, DateTime.Now);
            Town _t = gm.CreateTown(player);
            _t.player = player;
        }

        public override async Task<Individual> PlayAsync(CancellationToken ct) {
            return i;
        }
    }
}
