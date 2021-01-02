using System;
using System.Collections.Generic;
using System.Numerics;

namespace SharedLibrary.Models {

    public class QuadTree {
        int MAX_CAPACITY = 4;
        private int level = 0;
        private List<TreeNode> treeNodes;
        private QuadTree northWest = null;
        private QuadTree northEast = null;
        private QuadTree southWest = null;
        private QuadTree southEast = null;
        private TreeBoundry boundry;

        public QuadTree(int _level, TreeBoundry _boundry) {
            level = _level;
            treeNodes = new List<TreeNode>();
            boundry = _boundry;
        }

        void Split() {
            int _xOffset = boundry.xMin
                    + (boundry.xMax - boundry.xMin) / 2;
            int _zOffset = boundry.zMin
                    + (boundry.zMax - boundry.zMin) / 2;

            northWest = new QuadTree(level + 1, new TreeBoundry(
                    boundry.xMin,
                    boundry.zMin,
                    _xOffset,
                    _zOffset));
            northEast = new QuadTree(level + 1, new TreeBoundry(
                    _xOffset,
                    boundry.zMin,
                    boundry.xMax,
                    _zOffset));
            southWest = new QuadTree(level + 1, new TreeBoundry(
                    boundry.xMin,
                    _zOffset,
                    _xOffset,
                    boundry.zMax));
            southEast = new QuadTree(level + 1, new TreeBoundry(
                    _xOffset,
                    _zOffset,
                    boundry.xMax,
                    boundry.zMax));

        }

        /// <summary>
        /// Inserts a node into the quadtree
        /// </summary>
        /// <param name="_treeNode">The node to insert</param>
        public void Insert(TreeNode _treeNode) {
            float _x = _treeNode.position.X;
            float _z = _treeNode.position.Z;
            if (!boundry.inRange(_x, _z)) {
                return;
            }

            if (treeNodes.Count < MAX_CAPACITY) {
                treeNodes.Add(_treeNode);
                return;
            }
            // Exceeded the capacity so split it in FOUR
            if (northWest == null) {
                Split();
            }

            // Check to which partition coordinates belong
            if (northWest.boundry.inRange(_x, _z))
                northWest.Insert(_treeNode);
            else if (northEast.boundry.inRange(_x, _z))
                northEast.Insert(_treeNode);
            else if (southWest.boundry.inRange(_x, _z))
                southWest.Insert(_treeNode);
            else if (southEast.boundry.inRange(_x, _z))
                southEast.Insert(_treeNode);
            else
                Console.WriteLine($"ERROR : Unhandled partition {_x} {_z}");
        }

        /// <summary>
        /// Searches and returns all content of the quadtree within the specified bounds
        /// </summary>
        /// <param name="_tree">The quadtree to search in</param>
        /// <param name="_startX">start x</param>
        /// <param name="_startZ">start z</param>
        /// <param name="_endX">end x</param>
        /// <param name="_endZ">end z</param>
        /// <param name="_wholeMap">List of all found objects</param>
        private void GetAreaContent(QuadTree _tree, int _startX, int _startZ, int _endX, int _endZ, List<TreeNode> _wholeMap) {
            if (_tree == null) return;

            if (!(_startX > _tree.boundry.xMax) && !(_endX < _tree.boundry.xMin) && !(_startZ > _tree.boundry.zMax) && !(_endZ < _tree.boundry.zMin)) {
                foreach (TreeNode _treeNode in _tree.treeNodes) {
                    if (_treeNode.InRange(_startX, _startZ, _endX, _endZ)) {
                        _wholeMap.Add(_treeNode);
                    }
                }
            }
            GetAreaContent(_tree.northWest, _startX, _startZ, _endX, _endZ, _wholeMap);
            GetAreaContent(_tree.northEast, _startX, _startZ, _endX, _endZ, _wholeMap);
            GetAreaContent(_tree.southWest, _startX, _startZ, _endX, _endZ, _wholeMap);
            GetAreaContent(_tree.southEast, _startX, _startZ, _endX, _endZ, _wholeMap);
        }

        public List<TreeNode> GetAllContentBetween(int _startX, int _startZ, int _endX, int _endZ) {
            List<TreeNode> _wholeMap = new List<TreeNode>();
            GetAreaContent(this, _startX, _startZ, _endX, _endZ, _wholeMap);
            return _wholeMap;
        }

        /// <summary>
        /// Adds the attack or support reference between two towns
        /// </summary>
        /// <param name="_atkTown">the origin of the action</param>
        /// <param name="_deffTown">the target of the action</param>
        public void AddTownActionReference(Town _atkTown, Town _deffTown) {
            if (_atkTown.player == _deffTown.player) {
                if (!_deffTown.supporterTowns.Contains(_atkTown)) {
                    _deffTown.AddSupporterTown(_atkTown);
                }
            }
            else {
                if (!_deffTown.attackerTowns.Contains(_atkTown)) {
                    _deffTown.AddAttackTown(_atkTown);
                }
            }
            _atkTown.AddOutgoingTown(_deffTown);
        }

        /// <summary>
        /// Removes the attack or support reference between two towns
        /// </summary>
        /// <param name="_atkTown">the origin of the action</param>
        /// <param name="_deffTown">the target of the action</param>
        public void RmTownActionReference(Town _atkTown, Town _deffTown) {
            if (_atkTown.player == _deffTown.player) {
                _deffTown.RemoveSupporterTown(_atkTown);
            }
            else {
                _deffTown.RemoveAttackTown(_atkTown);
            }
            _atkTown.RemoveOutgoingTown(_deffTown);
        }

        /// <summary>
        /// Updates the owner of a town when conquered
        /// </summary>
        /// <param name="_player">The player who conquered the town</param>
        /// <param name="_town">The town which is conquered</param>
        public void UpdateOwner(Player _player, Town _town) {
            Player _oldOwner = _town.player;
            _oldOwner.towns.Remove(_town);

            _town.creationTime = DateTime.Now;
            _town.player = _player;
            _player.towns.Add(_town);
        }

        /// <summary>
        /// Searches a town within the quadtree and returns it
        /// </summary>
        /// <param name="_tree">The quadtree</param>
        /// <param name="_town">The coord of the town to look for</param>
        /// <returns>the town object or null if no town is found</returns>
        public Town SearchTown(QuadTree _tree, Vector3 _town) {

            if (_tree == null) return null;

            if (!(_town.X > _tree.boundry.xMax) && !(_town.X < _tree.boundry.xMin) && !(_town.Z > _tree.boundry.zMax) && !(_town.Z < _tree.boundry.zMin)) {
                for (int i = 0; i < _tree.treeNodes.Count; i++) {
                    if (_tree.treeNodes[i].IsNode(_town.X, _town.Z)) {
                        return (Town)_tree.treeNodes[i];
                    }
                }
            }
            if (SearchTown(_tree.northWest, _town) != null) {
                return SearchTown(_tree.northWest, _town);
            }
            if (SearchTown(_tree.northEast, _town) != null) {
                return SearchTown(_tree.northEast, _town);
            }
            if (SearchTown(_tree.southWest, _town) != null) {
                return SearchTown(_tree.southWest, _town);
            }
            if (SearchTown(_tree.southEast, _town) != null) {
                return SearchTown(_tree.southEast, _town);
            }
            return null;
        }

    }
}
