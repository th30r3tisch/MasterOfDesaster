using System.Collections.Generic;

namespace SharedLibrary.Models {
    public class World {
        private QuadTree quadtree;

        public World(int _startX, int _startZ, int _endX, int _endZ) {
            quadtree = new QuadTree(1, new TreeBoundry(_startX, _startZ, _endX, _endZ));
        }

        public void Insert(TreeNode _treeNode) {
            quadtree.Insert(_treeNode);
        }

        public List<TreeNode> GetAreaContent(int _startX, int _startZ, int _endX, int _endZ) {
            return quadtree.GetAllContent(quadtree, _startX, _startZ, _endX, _endZ);
        }

        public QuadTree GetQuadtree() {
            return quadtree;
        }
    }
}
