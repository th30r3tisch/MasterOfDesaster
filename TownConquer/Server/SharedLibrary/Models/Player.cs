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

        public Player(int _id, string _username, Color _color, DateTime _creationTime) {
            id = _id;
            username = _username;
            color = _color;
            towns = new List<Town>();
            creationTime = _creationTime;
        }

        public void addTown(Town _t) {
            towns.Add(_t);
        }
    }
}
