using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    public GameObject townPrefab;
    public GameObject landPrefab;
    public GameObject obstaclePrefab;
    public Canvas ui;

    private static QuadTree _world;
    private static Player _game;
    private Quaternion _horizontalOrientation = new Quaternion(0, 0, 0, 0);
    private static System.Random _r;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void AddEnemies(Player enemy, List<Vector3> towns) {
        foreach (Vector3 townPos in towns) {
            UTown town = (UTown)_world.SearchTown(_world, ConversionManager.ToNumericVector( townPos));
            if (town == null) {
                CreateTown(Constants.TOWN_NUMBER + Client.instance.enemies.Count + 1, townPos, enemy);
            }
            else {
                TownManager tm = town.go.GetComponent<TownManager>();
                enemy.towns.Add(town);
                town.owner = enemy;
                tm.ownerid = enemy.id;
                tm.ownerName = enemy.username;
                town.go.GetComponent<Renderer>().material.color = ConversionManager.DrawingToColor32(enemy.color);
            }
        }
        Client.instance.enemies.Add(enemy);
    }

    public void InitMap(int seed, Vector3 townPos, Player player, DateTime creationTime) {
        GameObject ground;
        _r = new System.Random(seed);

        _world = new QuadTree(1, new TreeBoundry(0, 0, Constants.MAP_WIDTH, Constants.MAP_HEIGHT));
        _game = new Player(-1, "game", System.Drawing.Color.FromArgb(100, 100, 100), creationTime);
        ground = Instantiate(landPrefab, new Vector3(Constants.MAP_WIDTH / 2, 0, Constants.MAP_HEIGHT / 2), _horizontalOrientation);
        ground.transform.localScale = new Vector3(Constants.MAP_WIDTH, 1, Constants.MAP_HEIGHT);

        CreateObstacles();
        CreateTowns();

        CreateTown(Constants.TOWN_NUMBER, townPos, player);
        ui.GetComponent<GameUIManager>().Init();
    }

    private void CreateTowns() {
        for (int i = 0; i < Constants.TOWN_NUMBER; i++) {
            SearchTownPos(i);
        }
    }

    private void SearchTownPos(int i) {
        bool flag = false;
        while (flag == false) {
            int x = RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES);
            int z = RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES);
            if (_world.GetAllContentBetween(
                (x - Constants.TOWN_MIN_DISTANCE),
                    (z - Constants.OBSTACLE_MAX_LENGTH / 2), // divided by 2 because point is center of object
                    (x + Constants.TOWN_MIN_DISTANCE),
                    (z + Constants.OBSTACLE_MAX_LENGTH / 2)).Count == 0) { // check vertical objects
                if (_world.GetAllContentBetween(
                    (x - Constants.OBSTACLE_MAX_LENGTH / 2),
                    (z - Constants.TOWN_MIN_DISTANCE),
                    (x + Constants.OBSTACLE_MAX_LENGTH / 2),
                    (z + Constants.TOWN_MIN_DISTANCE)).Count == 0) { // check horizontal objects
                    CreateTown(i, new Vector3(x, 5, z), _game);
                    flag = true;
                }
            }
        }
    }

    private void CreateTown(int i, Vector3 position, Player player) {
        GameObject town;
        UTown t;

        t = new UTown(ConversionManager.ToNumericVector(position)) {
            owner = player
        };

        town = Instantiate(townPrefab, position, _horizontalOrientation);
        town.GetComponent<TownManager>().id = i;
        town.GetComponent<TownManager>().ownerName = player.username;
        town.GetComponent<TownManager>().ownerid = player.id;
        town.GetComponent<TownManager>().life = t.life;
        town.GetComponent<TownManager>().town = t;
        town.GetComponent<Renderer>().material.color = ConversionManager.DrawingToColor32(player.color);

        t.go = town;
        t.creationTime = player.creationTime;
        player.towns.Add(t);
        _world.Insert(t);
    }

    private void CreateObstacles() {
        GameObject obstacle;
        Obstacle o;
        for (int i = 0; i < Constants.OBSTACLE_NUMBER; i++) {
            Vector3 position = new Vector3(
                        RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES),
                        1,
                        RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES));
            int orientation = RandomNumber(0, 1);
            int length = RandomNumber(Constants.OBSTACLE_MIN_LENGTH, Constants.OBSTACLE_MAX_LENGTH);
            o = new Obstacle(ConversionManager.ToNumericVector(position), orientation, length);
            _world.Insert(o);
            obstacle = Instantiate(obstaclePrefab, position, _horizontalOrientation);
            obstacle.transform.localScale = new Vector3(o.width, 8, o.length);
        }
    }

    public void AddInteractionToTown(Vector3 lineStart, Vector3 lineEnd) {
        UTown deffT = (UTown)_world.SearchTown(_world, ConversionManager.ToNumericVector(lineEnd));
        UTown atkT = (UTown)_world.SearchTown(_world, ConversionManager.ToNumericVector(lineStart));
        GameObject line;

        if (deffT.owner.id == atkT.owner.id) {
            line = CreateLineMesh(atkT.owner.id, atkT, deffT, "sup");
        }
        else {
            line = CreateLineMesh(atkT.owner.id, atkT, deffT, "atk");
        }
        atkT.outgoingActions.Add(line);
        deffT.AddTownActionReference(atkT);
    }

    public void RetreatTroops(Vector3 lineStart, Vector3 lineEnd) {
        UTown atkT = (UTown)_world.SearchTown(_world, ConversionManager.ToNumericVector(lineStart));
        UTown deffT = (UTown)_world.SearchTown(_world, ConversionManager.ToNumericVector(lineEnd));
        TownManager atkTm = atkT.go.GetComponent<TownManager>();
        atkTm.RetreatTroopsFromTown(deffT);

        deffT.RmTownActionReference(atkT);
    }

    public void ConquerTown(int conquererId, Vector3 conqueredTownCoord) {
        UTown conqueredT = (UTown)_world.SearchTown(_world, ConversionManager.ToNumericVector(conqueredTownCoord));
        Player conquerer = GetPlayer(conquererId);

        UpdateTownReferences(conqueredT, conquerer);
    }

    /// <summary>
    /// Searches and returns the player with the given id
    /// </summary>
    /// <param name="id">Id of the searched player</param>
    /// <returns>Player with the id or null if no player is found</returns>
    private Player GetPlayer(int id) {
        foreach (Player player in Client.instance.enemies) {
            if (player.id == id) {
                return player;
            }
        }
        if (id == Client.instance.myId) {
            return Client.instance.me;
        }
        return null;
    }

    private void UpdateTownReferences(UTown conqueredT, Player conquerer) {
        TownManager conqueredTm = conqueredT.go.GetComponent<TownManager>();

        // removes all attacking troops and deletes references in both towns
        for (int i = conqueredT.incomingAttackerTowns.Count; i > 0; i--) {
            UTown atkT = (UTown)conqueredT.incomingAttackerTowns[i - 1];
            atkT.go.GetComponent<TownManager>().RetreatTroopsFromTown(conqueredT);
            conqueredT.RmTownActionReference(atkT);
        }

        // removes all attacking troops and deletes references in both towns
        for (int i = conqueredT.incomingSupporterTowns.Count; i > 0; i--) {
            UTown supT = (UTown)conqueredT.incomingSupporterTowns[i - 1];
            supT.go.GetComponent<TownManager>().RetreatTroopsFromTown(conqueredT);
            conqueredT.RmTownActionReference(supT);
        }

        // removes all outgoing troops and deletes references in both towns
        for (int i = conqueredT.outgoingActionsToTowns.Count; i > 0; i--) {
            UTown targetT = (UTown)conqueredT.outgoingActionsToTowns[i - 1];
            conqueredTm.RetreatTroopsFromTown(targetT);
            targetT.RmTownActionReference(conqueredT);
        }

        conqueredTm.ownerid = conquerer.id;
        conqueredTm.ownerName = conquerer.username;
        conqueredT.UpdateOwner(conquerer);
        conqueredT.go.GetComponent<Renderer>().material.color = ConversionManager.DrawingToColor32(conquerer.color);
    }

    /// <summary>
    /// Creates a mesh line that indicates an attack between towns.
    /// </summary>
    /// <param name="ownerId">Player id who sent the attack</param>
    /// <param name="startTown">Town from where the attack starts</param>
    /// <param name="endTown">Town where the attack ends</param>
    /// <param name="type">Type of the line (atk, deff)</param>
    /// <returns>The Gameobject of the mesh line</returns>
    private GameObject CreateLineMesh(int ownerId, UTown startTown, UTown endTown, string type) {
        GameObject atkLine = new GameObject();
        MeshRenderer meshRenderer = atkLine.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = atkLine.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        Vector3 lineStart = ConversionManager.ToUnityVector( startTown.position);
        Vector3 lineEnd = ConversionManager.ToUnityVector(endTown.position);
        Vector3 direction = lineEnd - lineStart;

        meshRenderer.sharedMaterial = Resources.Load("Line", typeof(Material)) as Material;

        Vector3[] _vertices = new Vector3[4]{
            Vector3.Cross(direction, Vector3.up).normalized * Constants.ATTACK_LINE_WIDTH + lineStart,
            Vector3.Cross(direction, Vector3.up).normalized * (-Constants.ATTACK_LINE_WIDTH) + lineStart,
            Vector3.Cross(direction, Vector3.up).normalized * Constants.ATTACK_LINE_WIDTH + lineEnd,
            Vector3.Cross(direction, Vector3.up).normalized * (-Constants.ATTACK_LINE_WIDTH) + lineEnd
        };
        int[] _tris = new int[6]{
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        Vector3[] normals = new Vector3[4]{
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        Vector2[] uv = new Vector2[4]{
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        mesh.vertices = _vertices;
        mesh.triangles = _tris;
        mesh.normals = normals;
        mesh.uv = uv;
        meshFilter.mesh = mesh;
        atkLine.name = "atk";
        atkLine.AddComponent<AttackManager>();
        atkLine.GetComponent<AttackManager>().ownerid = ownerId;
        atkLine.GetComponent<AttackManager>().start = startTown;
        atkLine.GetComponent<AttackManager>().end = endTown;
        atkLine.GetComponent<AttackManager>().type = type;
        atkLine.AddComponent<MeshCollider>();

        return atkLine;
    }

    /// <summary>
    /// Generates a random number within the bounds min/max.
    /// </summary>
    /// <param name="min">min random number created</param>
    /// <param name="max">max random number created</param>
    /// <returns>The created random number</returns>
    private static int RandomNumber(int min, int max) {
        return _r.Next(max - min + 1) + min;
    }
}
