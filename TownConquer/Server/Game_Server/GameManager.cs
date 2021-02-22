﻿using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using SharedLibrary;
using System.Drawing;
using Game_Server.KI;
using System.Threading;
using System.Threading.Tasks;
using Game_Server.EA.Models.Simple;

namespace Game_Server {
    class GameManager {
        public Game game;
        
        public readonly object treeLock = new object();

        public static void Update() {
            ThreadManager.UpdateMain();
        }

        public GameManager(Game associatedGame) {
            game = associatedGame;
            GenereateInitialMap();
        }

        public void GenereateInitialMap() {
            game.InitData(
                new QuadTree(1, new TreeBoundry(0, 0, Constants.MAP_WIDTH, Constants.MAP_HEIGHT)),
                new Player(-1, "game", Color.FromArgb(150, 150, 150), DateTime.Now),
                new Random(Constants.RANDOM_SEED)
                );

            CreateObstacles();
            CreateTowns();
        }

        private void CreateTowns() {
            for (int i = 0; i < Constants.TOWN_NUMBER; i++) {
                CreateTown(game.initOwner);
            }
        }

        public Town CreateTown(Player owner) {
            Town t = null;
            QuadTree tree = game.tree;
            while (t == null) {
                int x = RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES);
                int z = RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES);

                if (tree.GetAllContentBetween(
                    (x - Constants.TOWN_MIN_DISTANCE),
                    (z - Constants.OBSTACLE_MAX_LENGTH / 2), // divided by 2 because point is center of object
                    (x + Constants.TOWN_MIN_DISTANCE),
                    (z + Constants.OBSTACLE_MAX_LENGTH / 2)).Count == 0) { // check vertical objects
                    if (tree.GetAllContentBetween(
                        (x - Constants.OBSTACLE_MAX_LENGTH / 2),
                        (z - Constants.TOWN_MIN_DISTANCE),
                        (x + Constants.OBSTACLE_MAX_LENGTH / 2),
                        (z + Constants.TOWN_MIN_DISTANCE)).Count == 0) { // check horizontal objects
                        t = new Town(new Vector3(x, 5, z));
                    }
                }
            }
            t.owner = owner;
            t.creationTime = owner.creationTime;
            owner.towns.Add(t);
            tree.Insert(t);
            return t;
        }

        private void CreateObstacles() {
            QuadTree tree = game.tree;
            for (int i = 0; i < Constants.OBSTACLE_NUMBER; i++) {
                tree.Insert(new Obstacle(
                        new Vector3(
                            RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES),
                            2,
                            RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES)),
                            RandomNumber(0, 1),
                            RandomNumber(Constants.OBSTACLE_MIN_LENGTH, Constants.OBSTACLE_MAX_LENGTH)));
            }
        }

        public bool IsIntersecting(Vector3 atkTown, Vector3 deffTown) {
            QuadTree tree = game.tree;
            List<TreeNode> intersectionObjs = new List<TreeNode>();
            int t1x = (int)atkTown.X;
            int t1z = (int)atkTown.Z;
            int t2x = (int)deffTown.X;
            int t2z = (int)deffTown.Z;
            int startX = Math.Min(t1x, t2x);
            int startZ = Math.Min(t1z, t2z);
            int endX = Math.Max(t1x, t2x);
            int endZ = Math.Max(t1z, t2z);
            //rectangle vertical
            intersectionObjs.AddRange(tree.GetAllContentBetween(
                startX,
                startZ - Constants.OBSTACLE_MAX_LENGTH / 2,
                endX,
                endZ + Constants.OBSTACLE_MAX_LENGTH / 2));
            //rectangle horizontal
            intersectionObjs.AddRange(tree.GetAllContentBetween(
                startX - Constants.OBSTACLE_MAX_LENGTH / 2,
                startZ,
                endX + Constants.OBSTACLE_MAX_LENGTH / 2,
                endZ));
            if (intersectionObjs.Count != 0) {
                foreach (TreeNode node in intersectionObjs) {
                    if (node.GetType() == typeof(Obstacle)) {

                        Obstacle o = (Obstacle)node;
                        bool intersecting;

                        if (o.orientation == 1) { // horizontal
                            intersecting = LineSegmentsIntersection(
                                new Vector2(atkTown.X, atkTown.Z),
                                new Vector2(deffTown.X, deffTown.Z),
                                new Vector2(node.position.X - (o.width / 2), node.position.Z),
                                new Vector2(node.position.X + (o.width / 2), node.position.Z)
                                );
                        }
                        else { // vertical
                            intersecting = LineSegmentsIntersection(
                                new Vector2(atkTown.X, atkTown.Z),
                                new Vector2(deffTown.X, deffTown.Z),
                                new Vector2(node.position.X, node.position.Z - (o.length / 2)),
                                new Vector2(node.position.X, node.position.Z + (o.length / 2))
                                );
                        }

                        if (intersecting) return true;
                    }
                }
            }
            return false;
        }

        // https://github.com/setchi/Unity-LineSegmentsIntersection
        private bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
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

        private int RandomNumber(int min, int max) {
            return game.r.Next(max - min + 1) + min;
        }

        public void AddActionToTown(Vector3 atk, Vector3 deff, DateTime timeStamp) {
            
            QuadTree tree = game.tree;
            Town atkTown = tree.SearchTown(tree, atk);
            Town deffTown = tree.SearchTown(tree, deff);
            if (CanTownsInteract(atkTown, deffTown)) {
                lock (treeLock) {
                    atkTown.CalculateLife(timeStamp);
                    deffTown.CalculateLife(timeStamp);
                    deffTown.AddTownActionReference(atkTown);
                }
            }
        }

        public void RemoveActionFromTown(Vector3 atk, Vector3 deff, DateTime timeStamp) {
            QuadTree tree = game.tree;
            Town atkTown = tree.SearchTown(tree, atk);
            Town deffTown = tree.SearchTown(tree, deff);
            lock (treeLock) {
                atkTown.CalculateLife(timeStamp);
                deffTown.CalculateLife(timeStamp);
                deffTown.RmTownActionReference(atkTown);
            }
        }

        public void ConquerTown(Player player, Vector3 town, DateTime timeStamp) {
            QuadTree tree = game.tree;
            Town deffTown = tree.SearchTown(tree, town);
            lock (treeLock) {
                deffTown.CalculateLife(timeStamp);
                UpdateTown(deffTown, timeStamp);

                deffTown.UpdateOwner(player);
            }
        }

        /// <summary>
        /// updates one town and all its references
        /// </summary>
        /// <param name="town">town to update</param>
        /// <param name="timeStamp">time when update happened</param>
        private void UpdateTown(Town town, DateTime timeStamp) {
            lock (treeLock) {
                // removes all incoming atk troops and deletes references in both towns
                UpdateInteractingTowns(town.incomingAttackerTowns, town, timeStamp);
                // removes all incoming support troops and deletes references in both towns
                UpdateInteractingTowns(town.incomingSupporterTowns, town, timeStamp);

                // removes all outgoing troops and deletes references in both towns
                for (int i = town.outgoingActionsToTowns.Count; i > 0; i--) {
                    town.outgoingActionsToTowns[i - 1].CalculateLife(timeStamp);
                    town.outgoingActionsToTowns[i - 1].RmTownActionReference(town);
                }
            }
        }

        private void UpdateInteractingTowns(List<Town> interactingTowns, Town town, DateTime timeStamp) {
            for (int i = interactingTowns.Count; i > 0; i--) {
                interactingTowns[i - 1].CalculateLife(timeStamp);
                town.RmTownActionReference(interactingTowns[i - 1]);
            }
        }

        public bool CanTownsInteract(Town townOne, Town townTwo) {
            if (!townOne.outgoingActionsToTowns.Contains(townTwo) &&
                !townTwo.outgoingActionsToTowns.Contains(townOne) &&
                townTwo != townOne &&
                !IsIntersecting(townOne.position, townTwo.position)) {
                return true;
            }
            return false;
        }

        public void CreateKis() {
            var c = new CancellationTokenSource();
            var token = c.Token;

            KI_Base<Individual_Simple> ki1 = new KI_1(game, 999, "KI999", Color.FromArgb(255, 255, 255));
            KI_Base<Individual_Simple> ki2 = new KI_1(game, 998, "KI998", Color.FromArgb(0, 0, 0));
            //KI_Base<Individual_Advanced> ki2 = new KI_2(this, 998, "KI998", Color.FromArgb(0, 0, 0));

            Individual_Simple referenceIndividual = new Individual_Simple(999);
            Individual_Simple referenceIndividual2 = new Individual_Simple(998);
            //Individual_Advanced referenceIndividual2 = new Individual_Advanced(999);

            var t1 = ki1.SendIntoGame(token, referenceIndividual);
            var t2 = ki2.SendIntoGame(token, referenceIndividual2);

            Task.WhenAny(t1).ContinueWith(taskInfo => { 
                c.Cancel();
                Console.WriteLine($"{taskInfo.Result.Result.name} has won the game!");
            });
            
        }
    }
}
