using System;
using System.Drawing;

namespace GameServer.Models {

    [Serializable]
    class Player {
        public int id;
        public string username;
        public Color color;

        public Player(int _id, string _username, Color _color) {
            id = _id;
            username = _username;
            color = _color;
        }
    }
}
