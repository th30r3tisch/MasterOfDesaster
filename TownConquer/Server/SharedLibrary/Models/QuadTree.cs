using System;
using System.Collections.Generic;

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
            int _yOffset = boundry.yMin
                    + (boundry.yMax - boundry.yMin) / 2;

            northWest = new QuadTree(level + 1, new TreeBoundry(
                    boundry.xMin,
                    boundry.yMin,
                    _xOffset,
                    _yOffset));
            northEast = new QuadTree(level + 1, new TreeBoundry(
                    _xOffset,
                    boundry.yMin,
                    boundry.xMax,
                    _yOffset));
            southWest = new QuadTree(level + 1, new TreeBoundry(
                    boundry.xMin,
                    _yOffset,
                    _xOffset,
                    boundry.yMax));
            southEast = new QuadTree(level + 1, new TreeBoundry(
                    _xOffset,
                    _yOffset,
                    boundry.xMax,
                    boundry.yMax));

        }

        public void Insert(TreeNode _treeNode) {
            float _x = _treeNode.position.X;
            float _y = _treeNode.position.Y;
            if (!boundry.inRange(_x, _y)) {
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
            if (northWest.boundry.inRange(_x, _y))
                northWest.Insert(_treeNode);
            else if (northEast.boundry.inRange(_x, _y))
                northEast.Insert(_treeNode);
            else if (southWest.boundry.inRange(_x, _y))
                southWest.Insert(_treeNode);
            else if (southEast.boundry.inRange(_x, _y))
                southEast.Insert(_treeNode);
            else
                Console.WriteLine($"ERROR : Unhandled partition {_x} {_y}");
        }

        public void GetAreaContent(QuadTree _tree, int _startX, int _startY, int _endX, int _endY, List<TreeNode> _wholeMap) {
            if (_tree == null) return;

            if (!(_startX > _tree.boundry.xMax) && !(_endX < _tree.boundry.xMin) && !(_startY > _tree.boundry.yMax) && !(_endY < _tree.boundry.yMin)) {
                foreach (TreeNode _treeNode in _tree.treeNodes) {
                    if (_treeNode.InRange(_startX, _startY, _endX, _endY)) {
                        _wholeMap.Add(_treeNode);
                    }
                }
            }
            GetAreaContent(_tree.northWest, _startX, _startY, _endX, _endY, _wholeMap);
            GetAreaContent(_tree.northEast, _startX, _startY, _endX, _endY, _wholeMap);
            GetAreaContent(_tree.southWest, _startX, _startY, _endX, _endY, _wholeMap);
            GetAreaContent(_tree.southEast, _startX, _startY, _endX, _endY, _wholeMap);
        }

        public List<TreeNode> GetAllContent(QuadTree _tree, int _startX, int _startY, int _endX, int _endY) {
            List<TreeNode> _wholeMap = new List<TreeNode>();
            GetAreaContent(_tree, _startX, _startY, _endX, _endY, _wholeMap);
            return _wholeMap;
        }

        private void AddTownAtk(QuadTree _tree, Town _atk, Town _deff) {
            if (_tree == null) return;

            if (!(_deff.position.X > _tree.boundry.xMax) && !(_deff.position.X < _tree.boundry.xMin) && !(_deff.position.Y > _tree.boundry.yMax) && !(_deff.position.Y < _tree.boundry.yMin)) {
                for (int i = 0; i < _tree.treeNodes.Count; i++) {
                    if (_tree.treeNodes[i].IsNode(_deff.position.X, _deff.position.Y)) {
                        Town t = (Town)_tree.treeNodes[i];
                        if (t.GetAttackTowns().Count == 0) {
                            ((Town)_tree.treeNodes[i]).AddAttackTown(_atk);
                            ((Town)_tree.treeNodes[i]).life = _deff.life;
                        }
                        else {
                            foreach (Town _town in _deff.GetAttackTowns()) {
                                if (_town == _atk) return;
                                else {
                                    ((Town)_tree.treeNodes[i]).AddAttackTown(_atk);
                                    ((Town)_tree.treeNodes[i]).life = _deff.life;
                                }
                            }
                        }
                    }
                }
            }
            AddTownAtk(_tree.northWest, _deff, _atk);
            AddTownAtk(_tree.northEast, _deff, _atk);
            AddTownAtk(_tree.southWest, _deff, _atk);
            AddTownAtk(_tree.southEast, _deff, _atk);
        }

        private void RmTownAtk(QuadTree _tree, Town _deff, Town _atk) {
            if (_tree == null) return;

            if (!(_deff.position.X > _tree.boundry.xMax) && !(_deff.position.X < _tree.boundry.xMin) && !(_deff.position.Y > _tree.boundry.yMax) && !(_deff.position.Y < _tree.boundry.yMin)) {
                for (int i = 0; i < _tree.treeNodes.Count; i++) {
                    if (_tree.treeNodes[i].IsNode(_deff.position.X, _deff.position.Y)) {
                        foreach (Town _town in _deff.GetAttackTowns()) {
                            if (_town == _atk) {
                                ((Town)_tree.treeNodes[i]).RemoveAttackTown(_atk);
                                ((Town)_tree.treeNodes[i]).life = _deff.life;
                            }
                            else return;
                        }
                    }
                }
            }
            RmTownAtk(_tree.northWest, _deff, _atk);
            RmTownAtk(_tree.northEast, _deff, _atk);
            RmTownAtk(_tree.southWest, _deff, _atk);
            RmTownAtk(_tree.southEast, _deff, _atk);
        }



        private void UpdateOwner(QuadTree _tree, Player _player, TreeNode _treeNode) {
            if (_tree == null) return;

            if (!(_treeNode.position.X > _tree.boundry.xMax) && !(_treeNode.position.X < _tree.boundry.xMin) && !(_treeNode.position.Y > _tree.boundry.yMax) && !(_treeNode.position.Y < _tree.boundry.yMin)) {
                for (int i = 0; i < _tree.treeNodes.Count; i++) {
                    if (_tree.treeNodes[i].IsNode(_treeNode.position.X, _treeNode.position.X)) {
                        Console.WriteLine($"update owner: {_player.username}");
                        ((Town)_tree.treeNodes[i]).RemoveAllConquerors();
                        ((Town)_tree.treeNodes[i]).player = _player;
                        ((Town)_tree.treeNodes[i]).life = ((Town)_treeNode).life;

                        return;
                    }
                }
            }
            UpdateOwner(_tree.northWest, _player, _treeNode);
            UpdateOwner(_tree.northEast, _player, _treeNode);
            UpdateOwner(_tree.southWest, _player, _treeNode);
            UpdateOwner(_tree.southEast, _player, _treeNode);
        }

        public void AddUpdateNode(Town _attacker, Town _defender) {
            AddTownAtk(this, _attacker, _defender);
        }

        public void RmUpdateNode(List<TreeNode> _nodes) {
            Town inComingAtk = (Town)_nodes[0];
            Town inComingDeff = (Town)_nodes[1];
            RmTownAtk(this, inComingDeff, inComingAtk);
        }

        public void UpdateOwner(Player _player, TreeNode _treeNode) {
            UpdateOwner(this, _player, _treeNode);
        }
    }
}
