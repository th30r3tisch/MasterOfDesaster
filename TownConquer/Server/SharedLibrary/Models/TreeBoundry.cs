
namespace SharedLibrary.Models {
    public class TreeBoundry {
        public int xMin, zMin, xMax, zMax;

        public TreeBoundry(int _xMin, int _zMin, int _xMax, int _zMax) {
            xMin = _xMin;
            zMin = _zMin;
            xMax = _xMax;
            zMax = _zMax;
        }

        public bool inRange(float _x, float _z) {
            return (_x >= xMin && _x <= xMax
                    && _z >= zMin && _z <= zMax);
        }
    }
}
