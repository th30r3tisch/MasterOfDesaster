using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using SharedLibrary;
using System.Drawing;
using Game_Server.KI;

namespace Game_Server {
    class GameLogic {

        public static List<KI_base> kis;

        private static QuadTree world;
        private static Player game;
        private static Random r;

        public static void Update() {
            ThreadManager.UpdateMain();
        }

        public static QuadTree GenereateInitialMap() {
            world = new QuadTree(1, new TreeBoundry(0, 0, Constants.MAP_WIDTH, Constants.MAP_HEIGHT));
            game = new Player(-1, "game", Color.FromArgb(150, 150, 150), DateTime.Now);
            r = new Random(Constants.RANDOM_SEED);
            kis = new List<KI_base>();

            CreateObstacles();
            CreateTowns();
            return world;
        }

        private static void CreateTowns() {
            for (int _i = 0; _i < Constants.TOWN_NUMBER; _i++) {
                CreateTown(game);
            }
        }

        public static Town CreateTown(Player _owner) {
            Town _t = null;
            while (_t == null) {
                int _x = RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES);
                int _z = RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES);
                if (world.GetAllContentBetween(
                    (_x - Constants.TOWN_MIN_DISTANCE),
                    (_z - Constants.OBSTACLE_MAX_LENGTH / 2), // divided by 2 because point is center of object
                    (_x + Constants.TOWN_MIN_DISTANCE),
                    (_z + Constants.OBSTACLE_MAX_LENGTH / 2)).Count == 0) { // check vertical objects
                    if (world.GetAllContentBetween(
                        (_x - Constants.OBSTACLE_MAX_LENGTH / 2),
                        (_z - Constants.TOWN_MIN_DISTANCE),
                        (_x + Constants.OBSTACLE_MAX_LENGTH / 2),
                        (_z + Constants.TOWN_MIN_DISTANCE)).Count == 0) { // check horizontal objects
                        _t = new Town(new Vector3(_x, 5, _z));
                    }
                }
            }
            _t.player = _owner;
            _t.creationTime = _owner.creationTime;
            _owner.addTown(_t);
            world.Insert(_t);
            return _t;
        }

        private static void CreateObstacles() {
            for (int i = 0; i < Constants.OBSTACLE_NUMBER; i++) {
                world.Insert(new Obstacle(
                        new Vector3(
                            RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES),
                            2,
                            RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES)),
                        RandomNumber(0, 1),
                        RandomNumber(Constants.OBSTACLE_MIN_LENGTH, Constants.OBSTACLE_MAX_LENGTH)));
            }
        }

        public static bool IsIntersecting(Vector3 _atkTown, Vector3 _deffTown) {
            List<TreeNode> intersectionObjs = new List<TreeNode>();
            int t1x = (int)_atkTown.X;
            int t1z = (int)_atkTown.Z;
            int t2x = (int)_deffTown.X;
            int t2z = (int)_deffTown.Z;
            int startX = Math.Min(t1x, t2x);
            int startZ = Math.Min(t1z, t2z);
            int endX = Math.Max(t1x, t2x);
            int endZ = Math.Max(t1z, t2z);
            //rectangle vertical
            intersectionObjs.AddRange(world.GetAllContentBetween(
                startX,
                startZ - Constants.OBSTACLE_MAX_LENGTH / 2,
                endX,
                endZ + Constants.OBSTACLE_MAX_LENGTH / 2));
            //rectangle horizontal
            intersectionObjs.AddRange(world.GetAllContentBetween(
                startX - Constants.OBSTACLE_MAX_LENGTH / 2,
                startZ,
                endX + Constants.OBSTACLE_MAX_LENGTH / 2,
                endZ));
            if (intersectionObjs.Count != 0) {
                foreach (TreeNode _node in intersectionObjs) {
                    if (_node.GetType() == typeof(Obstacle)) {

                        Obstacle _o = (Obstacle)_node;
                        bool _intersecting = false;

                        if (_o.orientation == 1) { // horizontal
                            _intersecting = LineSegmentsIntersection(
                                new Vector2(_atkTown.X, _atkTown.Z),
                                new Vector2(_deffTown.X, _deffTown.Z),
                                new Vector2(_node.position.X - (_o.width / 2), _node.position.Z),
                                new Vector2(_node.position.X + (_o.width / 2), _node.position.Z)
                                );
                        }
                        else { // vertical
                            _intersecting = LineSegmentsIntersection(
                                new Vector2(_atkTown.X, _atkTown.Z),
                                new Vector2(_deffTown.X, _deffTown.Z),
                                new Vector2(_node.position.X, _node.position.Z - (_o.length / 2)),
                                new Vector2(_node.position.X, _node.position.Z + (_o.length / 2))
                                );
                        }

                        if (_intersecting) return true;
                    }
                }
            }
            return false;
        }

        // https://github.com/setchi/Unity-LineSegmentsIntersection
        public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
            Vector2 intersection = Vector2.Zero;

            var d = (p2.X - p1.X) * (p4.Y - p3.Y) - (p2.Y - p1.Y) * (p4.X - p3.X);

            if (d == 0.0f) {
                return false;
            }

            var u = ((p3.X - p1.X) * (p4.Y - p3.Y) - (p3.Y - p1.Y) * (p4.X - p3.X)) / d;
            var v = ((p3.X - p1.X) * (p2.Y - p1.Y) - (p3.Y - p1.Y) * (p2.X - p1.X)) / d;

            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f) {
                return false;
            }

            intersection.X = p1.X + u * (p2.X - p1.X);
            intersection.Y = p1.Y + u * (p2.Y - p1.Y);

            return true;
        }

        private static int RandomNumber(int _min, int _max) {
            return r.Next(_max - _min + 1) + _min;
        }

        public static void AddAttackToTown(Vector3 _atk, Vector3 _deff, DateTime _timeStamp) {
            Town _atkTown = world.SearchTown(world, _atk);
            Town _deffTown = world.SearchTown(world, _deff);
            CalculateTownLife(_atkTown, _timeStamp);
            CalculateTownLife(_deffTown, _timeStamp);
            world.AddTownAtk(_atkTown, _deffTown);
        }

        public static void ReomveAttackFromTown(Vector3 _atk, Vector3 _deff, DateTime _timeStamp) {
            Town _atkTown = world.SearchTown(world, _atk);
            Town _deffTown = world.SearchTown(world, _deff);
            CalculateTownLife(_atkTown, _timeStamp);
            CalculateTownLife(_deffTown, _timeStamp);
            world.RmTownAtk(_atkTown, _deffTown);
        }

        public static void ConquerTown(Player _player, Vector3 _town, DateTime _timeStamp) {
            Town _deffTown = world.SearchTown(world, _town);
            CalculateTownLife(_deffTown, _timeStamp);
            UpdateTown(_deffTown, _timeStamp);

            world.UpdateOwner(_player, _deffTown);
        }

        public static void UpdateTown(Town _town, DateTime _timeStamp) {
            // removes all incoming atk troops and deletes references in both towns
            for (int i = _town.attackerTowns.Count; i > 0; i--) {
                CalculateTownLife(_town.attackerTowns[i - 1], _timeStamp);
                world.RmTownAtk(_town.attackerTowns[i - 1], _town);
            }

            // removes all incoming support troops and deletes references in both towns
            for (int i = _town.supporterTowns.Count; i > 0; i--) {
                CalculateTownLife(_town.supporterTowns[i - 1], _timeStamp);
                world.RmTownAtk(_town.supporterTowns[i - 1], _town);
            }

            // removes all outgoing troops and deletes references in both towns
            for (int i = _town.outgoing.Count; i > 0; i--) {
                CalculateTownLife(_town.outgoing[i - 1], _timeStamp);
                world.RmTownAtk(_town, _town.outgoing[i - 1]);
            }
        }

        public static void CalculateTownLife(Town _town, DateTime _creationTime) {
            TimeSpan span = _creationTime.Subtract(_town.creationTime);
            int timePassed = (int)span.TotalSeconds;
            int firstLifeCalc = _town.life;

            if (_town.player.id != -1) {
                int rawTownLife = timePassed / Constants.TOWN_GROTH_SECONDS;
                int lostLifeByOutgoing = timePassed / Constants.TOWN_GROTH_SECONDS * _town.outgoing.Count;
                int gotLifeByIncoming = timePassed / Constants.TOWN_GROTH_SECONDS * _town.supporterTowns.Count;
                firstLifeCalc += rawTownLife - lostLifeByOutgoing + gotLifeByIncoming;
            }
            int lostLifeByIncoming = timePassed / Constants.TOWN_GROTH_SECONDS * _town.attackerTowns.Count;

            int finalNewLife = firstLifeCalc - lostLifeByIncoming;
            _town.life = finalNewLife;
            _town.creationTime = _creationTime;
            Console.WriteLine($"New town life is: {finalNewLife}");
        }

        public static void CreateKis() {
            kis.Add(new KI_Stupid(world, "KI1", Color.FromArgb(0, 0, 0)));
            kis.Add(new KI_Stupid(world, "KI2", Color.FromArgb(255, 255, 255)));
        }
    }
}
