using System.Numerics;

namespace SharedLibrary.Models {

    public class TreeNode {

        public Vector3 position;

        public TreeNode() { }

        public bool InRange(int startX, int startZ, int endX, int endZ) {
            return (position.X >= startX && position.X <= endX
                    && position.Z >= startZ && position.Z <= endZ);
        }

        public bool IsNode(float x, float z) {
            return (position.X == x && position.Z == z);
        }
    }
}
