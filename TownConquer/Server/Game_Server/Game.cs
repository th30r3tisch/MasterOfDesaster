using SharedLibrary.Models;
using System;
using System.Collections.Generic;

namespace Game_Server {
    class Game {

        public QuadTree tree;
        public Player initOwner;
        public Random r;

        public Game(QuadTree world, Player game, Random r) {
            tree = world;
            initOwner = game;
            this.r = r;
        }
    }
}
