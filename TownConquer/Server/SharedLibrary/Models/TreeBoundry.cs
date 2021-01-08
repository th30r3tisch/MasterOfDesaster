
namespace SharedLibrary.Models {
    public class TreeBoundry {
        public int xMin, zMin, xMax, zMax;

        public TreeBoundry(int xMin, int zMin, int xMax, int zMax) {
            this.xMin = xMin;
            this.zMin = zMin;
            this.xMax = xMax;
            this.zMax = zMax;
        }

        public bool InRange(float x, float z) {
            return (x >= xMin && x <= xMax
                    && z >= zMin && z <= zMax);
        }
    }
}
