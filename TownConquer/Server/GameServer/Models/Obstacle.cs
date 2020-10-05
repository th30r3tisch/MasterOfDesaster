using System;
using System.Numerics;

namespace GameServer.Models {

    [Serializable]
    class Obstacle : TreeNode{
        public int width;
        public int length;

        // orientation 0 is horizontal, 1 is vertical 
        public Obstacle(Vector3 _spawnPos, int _orientation, int _length) {
            position = _spawnPos;
            CreateObstacle(_orientation, _length);
        }

        private void CreateObstacle(int _orientation, int _obstacleLength) {
            if (_orientation == 0) {
                length = Constants.OBSTACLE_WIDTH;
                width = _obstacleLength;
            }
            else if (_orientation == 1) {
                length = _obstacleLength;
                width = Constants.OBSTACLE_WIDTH;
            }
        }
    }
}
