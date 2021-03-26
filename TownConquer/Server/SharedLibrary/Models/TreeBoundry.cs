
/// <summary>
/// BASIERT AUF DEM CODE VON Abhijeet Majumdar SIEHE:
/// Majumdar, Abhijeet, 1 Jul 2015, https://gist.github.com/AbhijeetMajumdar/c7b4f10df1b87f974ef4 [23.03.2021]
/// Der Code wurde bearbeitet und Anpassungen vorgenommen.
/// Dieser Code spielt im Rahmen der Arbeit nur eine geringe Rolle.
/// </summary>

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
