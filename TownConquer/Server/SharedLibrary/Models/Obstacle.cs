using System.Numerics;

namespace SharedLibrary.Models {

    public class Obstacle : TreeNode{
        public int width;
        public int length;
        public int orientation;

        // orientation 0 is horizontal, 1 is vertical 
        public Obstacle(Vector3 spawnPos, int orientation, int length) {
            position = spawnPos;
            CreateObstacle(orientation, length);
        }

        private void CreateObstacle(int orientation, int obstacleLength) {
            this.orientation = orientation;
            if (orientation == 0) {
                length = obstacleLength;
                width = Constants.OBSTACLE_WIDTH;
            }
            else if (orientation == 1) {
                length = Constants.OBSTACLE_WIDTH;
                width = obstacleLength;
            }
        }
    }
}
