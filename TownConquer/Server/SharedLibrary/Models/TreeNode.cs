using System.Numerics;

/// <summary>
/// BASIERT AUF DEM CODE VON Abhijeet Majumdar SIEHE:
/// Majumdar, Abhijeet, 1 Jul 2015, https://gist.github.com/AbhijeetMajumdar/c7b4f10df1b87f974ef4 [23.03.2021]
/// Der Code wurde bearbeitet und Anpassungen vorgenommen.
/// Dieser Code spielt im Rahmen der Arbeit nur eine geringe Rolle.
/// </summary>

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
