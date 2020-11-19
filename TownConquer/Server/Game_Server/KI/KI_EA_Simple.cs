using SharedLibrary.Models;
using System;
using System.Drawing;

namespace Game_Server.KI {
    class KI_EA_Simple : KI_base {

        public KI_EA_Simple(QuadTree _world, int id, string name, Color color) : base(_world) {
            player = new Player(id, name, color, DateTime.Now);
            Town _t = GameLogic.CreateTown(player);
            _t.player = player;
            kiThread.Start();
            Console.WriteLine($"KI_EA_Simple started.");
        }

        public override void Run() {
            throw new NotImplementedException();
        }
    }
}
