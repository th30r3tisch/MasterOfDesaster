using Game_Server.KI;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;

namespace Game_Server {
    class Game {

        public QuadTree tree;
        public Player initOwner;
        public Random r;
        public List<KI_base> kis;

        public Game(QuadTree _world, Player _game, Random _r) {
            tree = _world;
            initOwner = _game;
            r = _r;
            kis = new List<KI_base>();
        }
    }
}
