using System.Numerics;

namespace SharedLibrary.Models {

    public class Obstacle : TreeNode{
        public int width;
        public int length;
        public int orientation;

        // orientation 0 is horizontal, 1 is vertical 
        public Obstacle(Vector3 _spawnPos, int _orientation, int _length) {
            position = _spawnPos;
            CreateObstacle(_orientation, _length);
        }

        private void CreateObstacle(int _orientation, int _obstacleLength) {
            orientation = _orientation;
            if (_orientation == 0) {
                length = _obstacleLength;
                width = Constants.OBSTACLE_WIDTH;
            }
            else if (_orientation == 1) {
                length = Constants.OBSTACLE_WIDTH;
                width = _obstacleLength;
            }
        }
    }
}
