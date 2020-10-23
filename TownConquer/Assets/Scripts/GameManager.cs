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

    private static QuadTree world;
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

    public void AddEnemies(Player _enemy, Vector3 _townPos) {
        CreateTown(towns.Count, _townPos, _enemy);
        Client.instance.enemies.Add(_enemy);
    }

    public void InitMap(int _seed, Vector3 _townPos, Player _player) {
        GameObject _ground;
        r = new System.Random(_seed);

        world = new QuadTree(1, new TreeBoundry(0, 0, Constants.MAP_WIDTH, Constants.MAP_HEIGHT));
        game = new Player(-1, "game", System.Drawing.Color.FromArgb(100, 100, 100));
        _ground = Instantiate(landPrefab, new Vector3(Constants.MAP_WIDTH / 2, 0, Constants.MAP_HEIGHT / 2), horizontalOrientation);
        _ground.transform.localScale = new Vector3(Constants.MAP_WIDTH / 10, 1, Constants.MAP_HEIGHT / 10);

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
                (_x - Constants.TOWN_MIN_DISTANCE),
                    (_z - Constants.OBSTACLE_MAX_LENGTH / 2), // divided by 2 because point is center of object
                    (_x + Constants.TOWN_MIN_DISTANCE),
                    (_z + Constants.OBSTACLE_MAX_LENGTH / 2)).Count == 0) { // check vertical objects
                if (GetAreaContent(
                    (_x - Constants.OBSTACLE_MAX_LENGTH / 2),
                    (_z - Constants.TOWN_MIN_DISTANCE),
                    (_x + Constants.OBSTACLE_MAX_LENGTH / 2),
                    (_z + Constants.TOWN_MIN_DISTANCE)).Count == 0) { // check horizontal objects
                    CreateTown(_i, new Vector3(_x, 0, _z), game);
                    flag = true;
                }
            }
        }
    }

    private void CreateTown(int _i, Vector3 _position, Player owner) {
        GameObject _town;
        UTown _t;

        _t = new UTown(ConversionManager.ToNumericVector(_position)) {
            player = owner
        };

        _town = Instantiate(townPrefab, _position, horizontalOrientation);
        _town.GetComponent<TownManager>().id = _i;
        _town.GetComponent<TownManager>().ownerName = owner.username;
        _town.GetComponent<TownManager>().ownerid = owner.id;
        _town.GetComponent<TownManager>().life = _t.life;
        _town.GetComponentInChildren<Renderer>().material.color = ConversionManager.DrawingToColor32(owner.color);
        towns.Add(_i, _town.GetComponent<TownManager>());

        _t.go = _town;
        owner.addTown(_t);
        world.Insert(_t);

    }

    private void CreateObstacles() {
        GameObject _obstacle;
        Obstacle _o;
        for (int i = 0; i < Constants.OBSTACLE_NUMBER; i++) {
            Vector3 _position = new Vector3(
                        RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES),
                        0,
                        RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES));
            int _orientation = RandomNumber(0, 1);
            int _length = RandomNumber(Constants.OBSTACLE_MIN_LENGTH, Constants.OBSTACLE_MAX_LENGTH);
            _o = new Obstacle(ConversionManager.ToNumericVector(_position), _orientation, _length);
            world.Insert(_o);
            _obstacle = Instantiate(obstaclePrefab, _position, horizontalOrientation);
            _obstacle.transform.localScale = new Vector3(_o.width, 8, _o.length);
        }
    }

    private List<TreeNode> GetAreaContent(int _startX, int _startZ, int _endX, int _endZ) {
        return world.GetAllContentBetween(_startX, _startZ, _endX, _endZ);
    }

    public void AttackTown(Vector3 _lineStart, Vector3 _lineEnd) {
        UTown _deffT = (UTown)world.GetTown(ConversionManager.ToNumericVector(_lineEnd));
        UTown _atkT = (UTown)world.GetTown(ConversionManager.ToNumericVector(_lineStart));

        TownManager _deffTm = _deffT.go.GetComponent<TownManager>();
        TownManager _atkTm = _atkT.go.GetComponent<TownManager>();
        if (_deffTm.ownerid == _atkTm.ownerid) {
            _deffTm.supporter++;
        }
        else {
            _deffTm.attacker.Add(_atkT);
        }

        _deffT.incommingAttacks.Add(CreateLineMesh(_atkTm.ownerid, _lineStart, _lineEnd));
        world.AddTownAtk(ConversionManager.ToNumericVector(_lineStart), ConversionManager.ToNumericVector(_lineEnd));
    }

    public void RetreatTroops(Vector3 _lineStart, Vector3 _lineEnd) {
        UTown _atkT = (UTown)world.GetTown(ConversionManager.ToNumericVector(_lineStart));
        UTown _deffT = (UTown)world.GetTown(ConversionManager.ToNumericVector(_lineEnd));
        TownManager _tm = _deffT.go.GetComponent<TownManager>();

        if (_deffT.player.id == _atkT.player.id) {
            _tm.supporter--;
        }
        else {
            _tm.attacker.Remove(_atkT);
        }

        GameObject _attackToRemove = _deffT.GetAttackGameObject(_lineStart);
        DestroyImmediate(_attackToRemove);
        _deffT.incommingAttacks.Remove(_attackToRemove);
        world.RmTownAtk(ConversionManager.ToNumericVector(_lineStart), ConversionManager.ToNumericVector(_lineEnd));
    }

    public void ConquerTown(int _conquererId, Vector3 _deffTown) {
        if (_conquererId == Client.instance.myId) {
            UpdateTownReferences(_deffTown, Client.instance.me);
        }
        else {
            foreach (Player _player in Client.instance.enemies) {
                if (_player.id == _conquererId) {
                    UpdateTownReferences(_deffTown, _player);
                }
            }
        }
    }

    private void UpdateTownReferences(Vector3 _deffTown, Player _player) {
        UTown _t = (UTown)world.GetTown(ConversionManager.ToNumericVector(_deffTown));
        TownManager _tm = _t.go.GetComponent<TownManager>();

        _tm.attacker.Clear();
        _tm.supporter = 0;
        _tm.ownerid = _player.id;
        _tm.ownerName = _player.username;

        world.UpdateOwner(_player, ConversionManager.ToNumericVector(_deffTown));

        _t.go.GetComponentInChildren<Renderer>().material.color = ConversionManager.DrawingToColor32(_player.color);
        foreach (GameObject _gameObject in _t.incommingAttacks) {
            DestroyImmediate(_gameObject);
        }
        _t.incommingAttacks.Clear();
    }

    /// <summary>
    /// Creates a mesh line that indicates an attack between towns.
    /// </summary>
    /// <param name="_ownerId">Player id who sent the attack</param>
    /// <param name="_lineStart">Town coord from where the attack starts</param>
    /// <param name="_lineEnd">Town coord where the attack ends</param>
    /// <returns>The Gameobject of the mesh line</returns>
    private GameObject CreateLineMesh(int _ownerId, Vector3 _lineStart, Vector3 _lineEnd) {
        GameObject _atkLine = new GameObject();
        MeshRenderer _meshRenderer = _atkLine.AddComponent<MeshRenderer>();
        _meshRenderer.sharedMaterial = Resources.Load("Line", typeof(Material)) as Material;
        MeshFilter _meshFilter = _atkLine.AddComponent<MeshFilter>();
        Mesh _mesh = new Mesh();

        Vector3 _direction = _lineEnd - _lineStart;

        Vector3[] _vertices = new Vector3[4]{
            Vector3.Cross(_direction, Vector3.up).normalized * Constants.ATTACK_LINE_WIDTH + _lineStart,
            Vector3.Cross(_direction, Vector3.up).normalized * (-Constants.ATTACK_LINE_WIDTH) + _lineStart,
            Vector3.Cross(_direction, Vector3.up).normalized * Constants.ATTACK_LINE_WIDTH + _lineEnd,
            Vector3.Cross(_direction, Vector3.up).normalized * (-Constants.ATTACK_LINE_WIDTH) + _lineEnd
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
        _atkLine.AddComponent<AttackManager>();
        _atkLine.GetComponent<AttackManager>().ownerid = _ownerId;
        _atkLine.GetComponent<AttackManager>().start = _lineStart;
        _atkLine.GetComponent<AttackManager>().end = _lineEnd;
        _atkLine.AddComponent<MeshCollider>();

        return _atkLine;
    }

    /// <summary>
    /// Generates a random number within the bounds min/max.
    /// </summary>
    /// <param name="_min">min random number created</param>
    /// <param name="_max">max random number created</param>
    /// <returns>The created random number</returns>
    private static int RandomNumber(int _min, int _max) {
        return r.Next(_max - _min + 1) + _min;
    }
}
