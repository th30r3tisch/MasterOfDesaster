using SharedLibrary.Models;
using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Numerics;
using SharedLibrary;

namespace Game_Server {
    class GameLogic {

        private static World world;
        private static Random r;

        public static void Update() {
            ThreadManager.UpdateMain();
        }

        public static World GenereateInitialMap() {
            world = new World(0, 0, Constants.MAP_WIDTH, Constants.MAP_HEIGHT);
            r = new Random(Constants.RANDOM_SEED);
            CreateObstacles();
            CreateTowns();
            return world;
        }

        private static void CreateTowns() {
            for (int _i = 0; _i < Constants.TOWN_NUMBER; _i++) {
                CreateTown();
            }
        }

        public static Town CreateTown() {
            Town _t = null;
            while (_t == null) {
                int _x = RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES);
                int _z = RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES);
                if (GetAreaContent(
                    (_x - Constants.TOWN_MIN_DISTANCE), 
                    (_z - Constants.TOWN_MIN_DISTANCE), 
                    (_x + Constants.TOWN_MIN_DISTANCE), 
                    (_z + Constants.TOWN_MIN_DISTANCE)).Count == 0) { // check for overlapping towns
                    if (GetAreaContent(
                        (_x - Constants.OBSTACLE_MAX_LENGTH), 
                        (_z - Constants.OBSTACLE_MAX_LENGTH), 
                        (_x + Constants.TOWN_MIN_DISTANCE), 
                        (_z + Constants.TOWN_MIN_DISTANCE)).Count == 0) { // check for overlapping obstacles
                        _t = new Town(new Vector3(_x, 0, _z));
                    }
                }
            }
            world.Insert(_t);
            return _t;
        }

        private static void CreateObstacles() {
            for (int i = 0; i < Constants.OBSTACLE_NUMBER; i++) {
                world.Insert(new Obstacle(
                        new Vector3(
                            RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES),
                            RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES),
                            0),
                        RandomNumber(0, 1),
                        RandomNumber(Constants.OBSTACLE_MIN_LENGTH, Constants.OBSTACLE_MAX_LENGTH)));
            }
        }

        public bool IsIntersecting(List<TreeNode> _towns) {
            List<TreeNode> intersectionObjs = new List<TreeNode>();
            int t1x = (int)_towns[0].position.X;
            int t1z = (int)_towns[0].position.Z;
            int t2x = (int)_towns[1].position.X;
            int t2z = (int)_towns[1].position.Z;
            int startX = Math.Min(t1x, t2x);
            int startZ = Math.Min(t1z, t2z);
            int endX = Math.Max(t1x, t2x);
            int endZ = Math.Max(t1z, t2z);
            //rectangle between towns
            intersectionObjs.AddRange(GetAreaContent(startX, startZ, endX, endZ));
            //rectangle around town one
            intersectionObjs.AddRange(GetAreaContent(
                t1x - Constants.OBSTACLE_MAX_LENGTH, 
                t1z - Constants.OBSTACLE_MAX_LENGTH, 
                t1x + Constants.OBSTACLE_MAX_LENGTH, 
                t1z + Constants.OBSTACLE_MAX_LENGTH));
            //rectangle around town two
            intersectionObjs.AddRange(GetAreaContent(
                t2x - Constants.OBSTACLE_MAX_LENGTH, 
                t2z - Constants.OBSTACLE_MAX_LENGTH, 
                t2x + Constants.OBSTACLE_MAX_LENGTH, 
                t2z + Constants.OBSTACLE_MAX_LENGTH));
            if (intersectionObjs.Count != 0) {
                foreach (TreeNode _node in intersectionObjs) {
                    if (_node.GetType() == typeof(Obstacle)){
                        Point2D? _intersecting;
                        Line2D _line = new Line2D(
                            new Point2D(_towns[0].position.X, _towns[0].position.Z), 
                            new Point2D(_towns[1].position.X, _towns[0].position.Z));
                        _intersecting = _line.IntersectWith(
                            new Line2D(
                                new Point2D(_node.position.X, _node.position.Z), 
                                new Point2D(_node.position.X + ((Obstacle)_node).width, _node.position.Z + ((Obstacle)_node).length)));
                        if (_intersecting != null) return true;
                    }
                }
            }
            return false;
        }

        private static int RandomNumber(int _min, int _max) {
            return r.Next(_max - _min + 1) + _min;
        }

        public static List<TreeNode> GetAreaContent(int _startX, int _startY, int _endX, int _endY) {
            return world.GetAreaContent(_startX, _startY, _endX, _endY);
        }
    }
}
