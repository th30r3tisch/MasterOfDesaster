using SharedLibrary.Models;
using System;
using System.Drawing;

namespace Game_Server.KI {
    class KI_EA_Simple : KI_base {

        public KI_EA_Simple(QuadTree _world, int id, string name, Color color, GameLogic logic) : base(_world) {
            player = new Player(id, name, color, DateTime.Now);
            Town _t = Server.logic.CreateTown(player);
            _t.player = player;
            kiThread.Start(logic);
            Console.WriteLine($"KI_EA_Simple started.");
        }

        public override void Run(object logic) {
            throw new NotImplementedException();
        }
    }
}
