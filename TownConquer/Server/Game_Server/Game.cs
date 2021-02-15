using SharedLibrary.Models;
using System;
using System.Collections.Generic;

namespace Game_Server {
    class Game {

        public int id;
        public QuadTree tree;
        public Player initOwner;
        public Random r;
        
        public GameManager gm { get; set; }
        public Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public Dictionary<int, User> kis = new Dictionary<int, User>();

        public Game(int gameId) {
            gm = new GameManager(this);
            id = gameId;
        }

        public void InitData(QuadTree world, Player game, Random r ) {
            tree = world;
            initOwner = game;
            this.r = r;
        }
    }
}
