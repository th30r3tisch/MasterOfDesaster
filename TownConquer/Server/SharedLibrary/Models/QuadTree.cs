using System;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// BASIERT AUF DEM CODE VON Abhijeet Majumdar SIEHE:
/// Majumdar, Abhijeet, 1 Jul 2015, https://gist.github.com/AbhijeetMajumdar/c7b4f10df1b87f974ef4 [23.03.2021]
/// Der Code wurde bearbeitet und Anpassungen vorgenommen.
/// Dieser Code spielt im Rahmen der Arbeit nur eine geringe Rolle.
/// </summary>

namespace SharedLibrary.Models {

    public class QuadTree {
        private const int MAX_CAPACITY = 4;

        private int _level = 0;
        private List<TreeNode> _treeNodes;
        private QuadTree _northWest = null;
        private QuadTree _northEast = null;
        private QuadTree _southWest = null;
        private QuadTree _southEast = null;
        private TreeBoundry _boundry;

        public QuadTree(int level, TreeBoundry boundry) {
            _level = level;
            _treeNodes = new List<TreeNode>();
            _boundry = boundry;
        }

        void Split() {
            int xOffset = _boundry.xMin
                    + (_boundry.xMax - _boundry.xMin) / 2;
            int zOffset = _boundry.zMin
                    + (_boundry.zMax - _boundry.zMin) / 2;

            _northWest = new QuadTree(_level + 1, new TreeBoundry(
                    _boundry.xMin,
                    _boundry.zMin,
                    xOffset,
                    zOffset));
            _northEast = new QuadTree(_level + 1, new TreeBoundry(
                    xOffset,
                    _boundry.zMin,
                    _boundry.xMax,
                    zOffset));
            _southWest = new QuadTree(_level + 1, new TreeBoundry(
                    _boundry.xMin,
                    zOffset,
                    xOffset,
                    _boundry.zMax));
            _southEast = new QuadTree(_level + 1, new TreeBoundry(
                    xOffset,
                    zOffset,
                    _boundry.xMax,
                    _boundry.zMax));

        }

        /// <summary>
        /// Inserts a node into the quadtree
        /// </summary>
        /// <param name="treeNode">The node to insert</param>
        public void Insert(TreeNode treeNode) {
            float x = treeNode.position.X;
            float z = treeNode.position.Z;
            if (!_boundry.InRange(x, z)) {
                return;
            }

            if (_treeNodes.Count < MAX_CAPACITY) {
                _treeNodes.Add(treeNode);
                return;
            }
            // Exceeded the capacity so split it in FOUR
            if (_northWest == null) {
                Split();
            }

            // Check to which partition coordinates belong
            if (_northWest._boundry.InRange(x, z))
                _northWest.Insert(treeNode);
            else if (_northEast._boundry.InRange(x, z))
                _northEast.Insert(treeNode);
            else if (_southWest._boundry.InRange(x, z))
                _southWest.Insert(treeNode);
            else if (_southEast._boundry.InRange(x, z))
                _southEast.Insert(treeNode);
            else
                Console.WriteLine($"ERROR : Unhandled partition {x} {z}");
        }

        /// <summary>
        /// Searches and returns all content of the quadtree within the specified bounds
        /// </summary>
        /// <param name="tree">The quadtree to search in</param>
        /// <param name="startX">start x</param>
        /// <param name="startZ">start z</param>
        /// <param name="endX">end x</param>
        /// <param name="endZ">end z</param>
        /// <param name="wholeMap">List of all found objects</param>
        private void GetAreaContent(QuadTree tree, int startX, int startZ, int endX, int endZ, List<TreeNode> wholeMap) {
            if (tree == null) return;

            if (!(startX > tree._boundry.xMax) && !(endX < tree._boundry.xMin) && !(startZ > tree._boundry.zMax) && !(endZ < tree._boundry.zMin)) {
                foreach (TreeNode treeNode in tree._treeNodes) {
                    if (treeNode.InRange(startX, startZ, endX, endZ)) {
                        wholeMap.Add(treeNode);
                    }
                }
            }
            GetAreaContent(tree._northWest, startX, startZ, endX, endZ, wholeMap);
            GetAreaContent(tree._northEast, startX, startZ, endX, endZ, wholeMap);
            GetAreaContent(tree._southWest, startX, startZ, endX, endZ, wholeMap);
            GetAreaContent(tree._southEast, startX, startZ, endX, endZ, wholeMap);
        }

        public List<TreeNode> GetAllContentBetween(int startX, int startZ, int endX, int endZ) {
            List<TreeNode> wholeMap = new List<TreeNode>();
            GetAreaContent(this, startX, startZ, endX, endZ, wholeMap);
            return wholeMap;
        }

        /// <summary>
        /// Searches a town within the quadtree and returns it
        /// </summary>
        /// <param name="tree">The quadtree</param>
        /// <param name="town">The coord of the town to look for</param>
        /// <returns>the town object or null if no town is found</returns>
        public Town SearchTown(QuadTree tree, Vector3 town) {

            if (tree == null) return null;

            if (!(town.X > tree._boundry.xMax) && !(town.X < tree._boundry.xMin) && !(town.Z > tree._boundry.zMax) && !(town.Z < tree._boundry.zMin)) {
                for (int i = 0; i < tree._treeNodes.Count; i++) {
                    if (tree._treeNodes[i].IsNode(town.X, town.Z)) {
                        return (Town)tree._treeNodes[i];
                    }
                }
            }
            if (SearchTown(tree._northWest, town) != null) {
                return SearchTown(tree._northWest, town);
            }
            if (SearchTown(tree._northEast, town) != null) {
                return SearchTown(tree._northEast, town);
            }
            if (SearchTown(tree._southWest, town) != null) {
                return SearchTown(tree._southWest, town);
            }
            if (SearchTown(tree._southEast, town) != null) {
                return SearchTown(tree._southEast, town);
            }
            return null;
        }

    }
}
