using System;
using System.Collections.Generic;
using System.Drawing;

namespace SharedLibrary.Models {

    public class Player {
        public int id;
        public string username;
        public Color color;
        public List<Town> towns;
        public DateTime creationTime;

        public Player(int id, string username, Color color, DateTime creationTime) {
            this.id = id;
            this.username = username;
            this.color = color;
            this.creationTime = creationTime;

            towns = new List<Town>();
        }
    }
}
