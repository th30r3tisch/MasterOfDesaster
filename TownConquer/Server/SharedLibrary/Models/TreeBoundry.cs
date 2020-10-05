
namespace SharedLibrary.Models {
    public class TreeBoundry {
        public int xMin, yMin, xMax, yMax;

        public TreeBoundry(int _xMin, int _yMin, int _xMax, int _yMax) {
            xMin = _xMin;
            yMin = _yMin;
            xMax = _xMax;
            yMax = _yMax;
        }

        public bool inRange(float _x, float _y) {
            return (_x >= xMin && _x <= xMax
                    && _y >= yMin && _y <= yMax);
        }
    }
}
