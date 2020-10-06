using System.Numerics;

namespace SharedLibrary.Models {

    public class TreeNode {

        public Vector3 position;

        public TreeNode() { }

        public bool InRange(int _startX, int _startY, int _endX, int _endY) {
            return (position.X >= _startX && position.X <= _endX
                    && position.Y >= _startY && position.Y <= _endY);
        }

        public bool IsNode(float _x, float _y) {
            return (position.X == _x && position.Y == _y);
        }
    }
}
