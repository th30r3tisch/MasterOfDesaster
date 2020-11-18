using SharedLibrary.Models;
using System;
using System.Threading;

namespace Game_Server.KI {
    abstract class KI_base {

        public Player player { get; set; }
        public QuadTree world { get; set; }

        public Thread kiThread;

        public KI_base(QuadTree _world) {
            world = _world;
            kiThread = new Thread(Run);
        }

        public abstract void Run();
    }
}
