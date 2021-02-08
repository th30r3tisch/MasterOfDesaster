using Game_Server.EA.Models;
using SharedLibrary.Models;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Server.KI {
    class KI_2 : KI_Base<Individual_Advanced> {

        public KI_2(GameManager gm, int id, string name, Color color) : base(gm, id, name, color) { }

        protected override void CheckKITownLifes(Town town) {
            throw new NotImplementedException();
        }

        protected override Task<Individual_Advanced> PlayAsync(CancellationToken ct) {
            throw new NotImplementedException();
        }
    }
}
