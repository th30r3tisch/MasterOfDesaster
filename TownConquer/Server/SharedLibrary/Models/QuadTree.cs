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

        public void AddTownAtk(Vector3 _atk, Vector3 _deff) {
            Town _atkTown = SearchTown(this, _atk);
            Town _deffTown = SearchTown(this, _deff);
            if (!_deffTown.attackerTowns.Contains(_atkTown)) {
                _deffTown.AddAttackTown(_atkTown);
            }
        }

        public void RmTownAtk(Vector3 _atk, Vector3 _deff) {
            Town _atkTown = SearchTown(this, _atk);
            Town _deffTown = SearchTown(this, _deff);
            _deffTown.RemoveAttackTown(_atkTown);
        }

        public void UpdateOwner(Player _player, Vector3 _t) {
            Town _town = SearchTown(this, _t);
            Console.WriteLine($"update owner: {_player.username}");
            _town.RemoveAllConquerors();
            _town.player = _player;
        }

        private Town SearchTown(QuadTree _tree, Vector3 _town) {

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

        public Town GetTown(Vector3 _town) {
            return SearchTown(this, _town);
        }
    }
}
