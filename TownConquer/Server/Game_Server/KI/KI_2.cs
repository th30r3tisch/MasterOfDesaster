using Game_Server.EA.Models;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    class KI_2 : KI_Base {

        public KI_2(GameManager gm, int id, string name, Color color) : base(gm, id, name, color) { }

        protected override Task<Individual_Simple> PlayAsync(CancellationToken ct) {
            throw new NotImplementedException();
        }
    }
}
