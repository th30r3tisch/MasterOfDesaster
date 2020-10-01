﻿using System.Numerics;

namespace GameServer {
    class Player {
        public int id;
        public string username;
        public Vector3 position;
        public Quaternion rotation;

        public Player(int _id, string _username, Vector3 _spawnPos, Quaternion _rotation) {
            id = _id;
            username = _username;
            position = _spawnPos;
            rotation = _rotation;
        }
    }
}
