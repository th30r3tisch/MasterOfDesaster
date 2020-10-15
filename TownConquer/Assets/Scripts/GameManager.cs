using SharedLibrary;
using SharedLibrary.Models;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;
    public static Dictionary<int, TownManager> towns = new Dictionary<int, TownManager>();

    public GameObject townPrefab;
    public GameObject landPrefab;
    public GameObject obstaclePrefab;

    private static World world;
    private static Player game;
    private Quaternion horizontalOrientation = new Quaternion(0, 0, 0, 0);
    private static System.Random r;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void AddEnemies( Player _enemy, Vector3 _townPos) {
        CreateTown(towns.Count, _townPos, _enemy);
        Client.instance.enemies.Add(_enemy);
    }

    public void InitMap(int _seed, Vector3 _townPos, Player _player) {
        GameObject _ground;
        r = new System.Random(_seed);

        world = new World(0, 0, Constants.MAP_WIDTH, Constants.MAP_HEIGHT);
        game = new Player(-1, "game", System.Drawing.Color.FromArgb(100,100,100));
        _ground = Instantiate(landPrefab, new Vector3(Constants.MAP_WIDTH/2, 0, Constants.MAP_HEIGHT/2), horizontalOrientation);
        _ground.transform.localScale = new Vector3(Constants.MAP_WIDTH/10, 1, Constants.MAP_HEIGHT/10);

        CreateObstacles();
        CreateTowns();

        CreateTown(towns.Count, _townPos, _player);
    }

    private void CreateTowns() {
        for (int _i = 0; _i < Constants.TOWN_NUMBER; _i++) {
            SearchTownPos(_i);
        }
    }

    private void SearchTownPos(int _i) {
        bool flag = false;
        while (flag == false) {
            int _x = RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES);
            int _z = RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES);
            if (GetAreaContent(
                _x - Constants.TOWN_MIN_DISTANCE,
                _z - Constants.TOWN_MIN_DISTANCE,
                _x + Constants.TOWN_MIN_DISTANCE,
                _z + Constants.TOWN_MIN_DISTANCE).Count == 0) { // check for overlapping towns
                if (GetAreaContent(
                    _x - Constants.OBSTACLE_MAX_LENGTH,
                    _z - Constants.OBSTACLE_MAX_LENGTH,
                    _x + Constants.TOWN_MIN_DISTANCE,
                    _z + Constants.TOWN_MIN_DISTANCE).Count == 0) { // check for overlapping obstacles
                    CreateTown(_i, new Vector3(_x, 0, _z), game);
                    flag = true;
                }
            }
        }
    }

    private void CreateTown(int _i, Vector3 _position, Player owner) {
        GameObject _town;
        Town _t;

        _t = new Town(new System.Numerics.Vector3(_position.x, _position.y, _position.z));
        _t.player = owner;
        owner.addTown(_t);
        world.Insert(_t);

        _town = Instantiate(townPrefab, _position, horizontalOrientation);
        _town.GetComponent<TownManager>().id = _i;
        _town.GetComponent<TownManager>().ownerName = owner.username;
        _town.GetComponent<TownManager>().ownerid = owner.id;
        _town.GetComponent<TownManager>().life = _t.life;
        _town.GetComponentInChildren<Renderer>().material.color = new Color32(owner.color.R, owner.color.G, owner.color.B, owner.color.A );
        towns.Add(_i, _town.GetComponent<TownManager>());
    }

    private void CreateObstacles() {
        GameObject _obstacle;
        Obstacle _o;
        for (int i = 0; i < Constants.OBSTACLE_NUMBER; i++) {
            System.Numerics.Vector3 _position = new System.Numerics.Vector3(
                        RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES),
                        0,
                        RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES));
            int _orientation = RandomNumber(0, 1);
            int _length = RandomNumber(Constants.OBSTACLE_MIN_LENGTH, Constants.OBSTACLE_MAX_LENGTH);
            _o = new Obstacle(_position, _orientation, _length);
            world.Insert(_o);
            _obstacle = Instantiate(obstaclePrefab, new Vector3(_position.X, _position.Y, _position.Z), horizontalOrientation);
            _obstacle.transform.localScale = new Vector3(_o.width, 8, _o.length);
        }
    }

    private List<TreeNode> GetAreaContent(int _startX, int _startZ, int _endX, int _endZ) {
        return world.GetAreaContent(_startX, _startZ, _endX, _endZ);
    }

    public void AttackTown(Vector3 lineStart, Vector3 lineEnd) {
        CreateLineMesh(lineStart, lineEnd);
    }

    private void CreateLineMesh(Vector3 lineStart, Vector3 lineEnd) {
        GameObject _atkLine = new GameObject();
        MeshRenderer _meshRenderer = _atkLine.AddComponent<MeshRenderer>();
        _meshRenderer.sharedMaterial = Resources.Load("Line", typeof(Material)) as Material;
        MeshFilter _meshFilter = _atkLine.AddComponent<MeshFilter>();
        Mesh _mesh = new Mesh();

        Vector3 _direction = lineEnd - lineStart;

        Vector3[] _vertices = new Vector3[4]{
            Vector3.Cross(_direction, Vector3.up).normalized * Constants.ATTACK_LINE_WIDTH + lineStart,
            Vector3.Cross(_direction, Vector3.up).normalized * (-Constants.ATTACK_LINE_WIDTH) + lineStart,
            Vector3.Cross(_direction, Vector3.up).normalized * Constants.ATTACK_LINE_WIDTH + lineEnd,
            Vector3.Cross(_direction, Vector3.up).normalized * (-Constants.ATTACK_LINE_WIDTH) + lineEnd
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

        _mesh.vertices = _vertices;
        _mesh.triangles = _tris;
        _mesh.normals = normals;
        _mesh.uv = uv;
        _meshFilter.mesh = _mesh;
        _atkLine.name = "atk";
    }

    private static int RandomNumber(int _min, int _max) {
        return r.Next(_max - _min + 1) + _min;
    }
}
