using System.Numerics;

namespace SharedLibrary.Models {

    public class TreeNode {

        public Vector3 position;

        public TreeNode() { }

        public bool InRange(int _startX, int _startZ, int _endX, int _endZ) {
            return (position.X >= _startX && position.X <= _endX
                    && position.Z >= _startZ && position.Z <= _endZ);
        }

        public bool IsNode(float _x, float _z) {
            return (position.X == _x && position.Z == _z);
        }
    }
}
