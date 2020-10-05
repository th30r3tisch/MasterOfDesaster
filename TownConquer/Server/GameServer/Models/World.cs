using System.Collections.Generic;

namespace GameServer.Models {
    class World {
        private QuadTree quadtree;

        public World(int _startX, int _startY, int _endX, int _endY) {
            quadtree = new QuadTree(1, new TreeBoundry(_startX, _startY, _endX, _endY));
        }

        public void Insert(TreeNode _treeNode) {
            quadtree.Insert(_treeNode);
        }

        public List<TreeNode> GetAreaContent(int _startX, int _startY, int _endX, int _endY) {
            return quadtree.GetAllContent(quadtree, _startX, _startY, _endX, _endY);
        }

        public QuadTree GetQuadtree() {
            return quadtree;
        }
    }
}
